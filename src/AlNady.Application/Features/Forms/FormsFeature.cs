using AlNady.Application.Features.Forms.DTOs;
using AlNady.Application.Interfaces;
using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using AlNady.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlNady.Application.Features.Forms.Commands
{
    public record CreateFormCommand(int ProgramId, int UserId, string Title, List<CreateFormFieldRequest> Fields) : IRequest<Result<FormDto>>;
    public record SubmitFormResponseCommand(int FormId, int UserId, List<SubmitFieldValueRequest> FieldValues) : IRequest<Result<FormResponseDto>>;
    public record UpdateResponseStatusCommand(int ResponseId, int AdminUserId, FormResponseStatus NewStatus) : IRequest<Result>;
}

namespace AlNady.Application.Features.Forms.Queries
{
    public record GetFormByProgramQuery(int ProgramId) : IRequest<Result<FormDto>>;
    public record GetMyEnrollmentsQuery(int UserId, int Page, int PageSize) : IRequest<Result<PagedResult<FormResponseDto>>>;
    public record GetProgramEnrollmentsQuery(int ProgramId, int UserId, int Page, int PageSize) : IRequest<Result<PagedResult<FormResponseDto>>>;
}

namespace AlNady.Application.Features.Forms.Handlers
{
    public class CreateFormCommandHandler : IRequestHandler<Commands.CreateFormCommand, Result<FormDto>>
    {
        private readonly IApplicationDbContext _context;
        public CreateFormCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result<FormDto>> Handle(Commands.CreateFormCommand request, CancellationToken ct)
        {
            var program = await _context.TrainingPrograms.Include(p => p.Trainer).Include(p => p.Academy).Include(p => p.Form)
                .FirstOrDefaultAsync(p => p.ProgramId == request.ProgramId, ct);
            if (program == null) return Result<FormDto>.NotFound("Program not found.");
            if (program.Form != null) return Result<FormDto>.Conflict("Form already exists for this program.");
            var isOwner = (program.Trainer?.UserId == request.UserId) || (program.Academy?.UserId == request.UserId);
            if (!isOwner) return Result<FormDto>.Forbidden("You don't own this program.");
            var form = new Form { ProgramId = request.ProgramId, Title = request.Title, CreatedAt = DateTime.UtcNow };
            form.Fields = request.Fields.Select(f => new FormField { Label = f.Label, FieldType = f.FieldType, IsRequired = f.IsRequired, DisplayOrder = f.DisplayOrder, Options = f.Options, Placeholder = f.Placeholder }).ToList();
            _context.Forms.Add(form);
            await _context.SaveChangesAsync(ct);
            return Result<FormDto>.Success(MapFormDto(form), 201);
        }

        internal static FormDto MapFormDto(Form f) => new(f.FormId, f.ProgramId, f.Title,
            f.Fields.Select(ff => new FormFieldDto(ff.FieldId, ff.Label, ff.FieldType.ToString(), ff.IsRequired, ff.DisplayOrder, ff.Options, ff.Placeholder)).ToList());
    }

    public class SubmitFormResponseCommandHandler : IRequestHandler<Commands.SubmitFormResponseCommand, Result<FormResponseDto>>
    {
        private readonly IApplicationDbContext _context; private readonly INotificationService _notifications; private readonly IEmailService _email;
        public SubmitFormResponseCommandHandler(IApplicationDbContext context, INotificationService notifications, IEmailService email) { _context = context; _notifications = notifications; _email = email; }

        public async Task<Result<FormResponseDto>> Handle(Commands.SubmitFormResponseCommand request, CancellationToken ct)
        {
            var form = await _context.Forms.Include(f => f.Fields).Include(f => f.Program)
                .FirstOrDefaultAsync(f => f.FormId == request.FormId, ct);
            if (form == null) return Result<FormResponseDto>.NotFound("Form not found.");
            if (form.Program.Status != ProgramStatus.Published) return Result<FormResponseDto>.Failure("Program is not accepting enrollments.");
            if (form.Program.AvailableSlots <= 0) return Result<FormResponseDto>.Failure("No available slots in this program.");
            if (await _context.FormResponses.AnyAsync(r => r.FormId == request.FormId && r.UserId == request.UserId, ct))
                return Result<FormResponseDto>.Conflict("You have already enrolled in this program.");
            if (await _context.Blacklists.AnyAsync(b => b.UserId == request.UserId && b.IsActive, ct))
                return Result<FormResponseDto>.Forbidden("Your account is suspended.");

            var requiredFields = form.Fields.Where(f => f.IsRequired).Select(f => f.FieldId).ToList();
            var missingRequired = requiredFields.Except(request.FieldValues.Select(fv => fv.FieldId)).ToList();
            if (missingRequired.Any()) return Result<FormResponseDto>.Failure($"Missing required fields: {string.Join(", ", missingRequired)}");

            var response = new FormResponse { FormId = request.FormId, UserId = request.UserId, Status = FormResponseStatus.Pending, SubmittedAt = DateTime.UtcNow };
            response.FieldValues = request.FieldValues.Select(fv => new FieldValue { FieldId = fv.FieldId, Value = fv.Value }).ToList();
            form.Program.AvailableSlots--;
            if (form.Program.AvailableSlots <= 0) form.Program.Status = ProgramStatus.Full;
            _context.FormResponses.Add(response);
            await _context.SaveChangesAsync(ct);

            var user = await _context.Users.FindAsync(new object[] { request.UserId }, ct);
            if (user != null) _ = Task.Run(async () => { await _notifications.SendAsync(request.UserId, "Enrollment Submitted", $"Your enrollment for '{form.Program.Title}' is under review.", NotificationType.Enrollment); await _email.SendEnrollmentConfirmationAsync(user.Email, user.FullName, form.Program.Title ?? "Program"); });

            return Result<FormResponseDto>.Success(new FormResponseDto(response.ResponseId, response.FormId, response.UserId, user?.FullName ?? "", response.Status.ToString(), response.SubmittedAt,
                response.FieldValues.Select(fv => new FieldValueDto(fv.FieldId, form.Fields.FirstOrDefault(f => f.FieldId == fv.FieldId)?.Label ?? "", fv.Value)).ToList()), 201);
        }
    }

    public class GetFormByProgramQueryHandler : IRequestHandler<Queries.GetFormByProgramQuery, Result<FormDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetFormByProgramQueryHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<FormDto>> Handle(Queries.GetFormByProgramQuery request, CancellationToken ct)
        {
            var form = await _context.Forms.AsNoTracking().Include(f => f.Fields.OrderBy(ff => ff.DisplayOrder)).FirstOrDefaultAsync(f => f.ProgramId == request.ProgramId, ct);
            if (form == null) return Result<FormDto>.NotFound("No enrollment form found for this program.");
            return Result<FormDto>.Success(CreateFormCommandHandler.MapFormDto(form));
        }
    }

    public class GetMyEnrollmentsQueryHandler : IRequestHandler<Queries.GetMyEnrollmentsQuery, Result<PagedResult<FormResponseDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetMyEnrollmentsQueryHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<PagedResult<FormResponseDto>>> Handle(Queries.GetMyEnrollmentsQuery request, CancellationToken ct)
        {
            var query = _context.FormResponses.AsNoTracking().Include(r => r.User).Include(r => r.Form).ThenInclude(f => f.Fields).Include(r => r.FieldValues).Where(r => r.UserId == request.UserId);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(r => r.SubmittedAt).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(r => new FormResponseDto(r.ResponseId, r.FormId, r.UserId, r.User.FullName, r.Status.ToString(), r.SubmittedAt,
                r.FieldValues.Select(fv => new FieldValueDto(fv.FieldId, r.Form.Fields.FirstOrDefault(f => f.FieldId == fv.FieldId)?.Label ?? "", fv.Value)).ToList())).ToList();
            return Result<PagedResult<FormResponseDto>>.Success(PagedResult<FormResponseDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class GetProgramEnrollmentsQueryHandler : IRequestHandler<Queries.GetProgramEnrollmentsQuery, Result<PagedResult<FormResponseDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetProgramEnrollmentsQueryHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<PagedResult<FormResponseDto>>> Handle(Queries.GetProgramEnrollmentsQuery request, CancellationToken ct)
        {
            var program = await _context.TrainingPrograms.Include(p => p.Trainer).Include(p => p.Academy)
                .FirstOrDefaultAsync(p => p.ProgramId == request.ProgramId, ct);
            if (program == null) return Result<PagedResult<FormResponseDto>>.NotFound("Program not found.");
            var isOwner = (program.Trainer?.UserId == request.UserId) || (program.Academy?.UserId == request.UserId);
            var isAdmin = await _context.Users.AnyAsync(u => u.UserId == request.UserId && u.Role == UserRole.Admin, ct);
            if (!isOwner && !isAdmin) return Result<PagedResult<FormResponseDto>>.Forbidden("Access denied.");
            var query = _context.FormResponses.AsNoTracking().Include(r => r.User).Include(r => r.Form).ThenInclude(f => f.Fields).Include(r => r.FieldValues).Where(r => r.Form.ProgramId == request.ProgramId);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(r => r.SubmittedAt).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(r => new FormResponseDto(r.ResponseId, r.FormId, r.UserId, r.User.FullName, r.Status.ToString(), r.SubmittedAt,
                r.FieldValues.Select(fv => new FieldValueDto(fv.FieldId, r.Form.Fields.FirstOrDefault(f => f.FieldId == fv.FieldId)?.Label ?? "", fv.Value)).ToList())).ToList();
            return Result<PagedResult<FormResponseDto>>.Success(PagedResult<FormResponseDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class UpdateResponseStatusCommandHandler : IRequestHandler<Commands.UpdateResponseStatusCommand, Result>
    {
        private readonly IApplicationDbContext _context; private readonly INotificationService _notifications;
        public UpdateResponseStatusCommandHandler(IApplicationDbContext context, INotificationService notifications) { _context = context; _notifications = notifications; }
        public async Task<Result> Handle(Commands.UpdateResponseStatusCommand request, CancellationToken ct)
        {
            var response = await _context.FormResponses.Include(r => r.User).Include(r => r.Form).ThenInclude(f => f.Program)
                .FirstOrDefaultAsync(r => r.ResponseId == request.ResponseId, ct);
            if (response == null) return Result.NotFound("Enrollment response not found.");
            response.Status = request.NewStatus;
            await _context.SaveChangesAsync(ct);
            await _notifications.SendAsync(response.UserId, "Enrollment Status Update", $"Your enrollment status changed to: {request.NewStatus}", NotificationType.Enrollment, ct: ct);
            return Result.Success();
        }
    }
}
