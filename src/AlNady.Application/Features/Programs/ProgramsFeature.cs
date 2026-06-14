using AlNady.Application.Features.Programs.DTOs;
using AlNady.Application.Interfaces;
using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using AlNady.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlNady.Application.Features.Programs.Commands
{
    public record CreateProgramCommand(int UserId, string? Title, string? Description, string? SportType, DateTime ProgramDate, TimeSpan ProgramTime, decimal Price, int Capacity, string? TrainingLocation, string? AgeCategory, string? GenderPreference) : IRequest<Result<ProgramDto>>;
    public record UpdateProgramCommand(int ProgramId, int UserId, string? Title, string? Description, DateTime ProgramDate, TimeSpan ProgramTime, decimal Price, int Capacity, string? TrainingLocation, ProgramStatus Status) : IRequest<Result<ProgramDto>>;
    public record DeleteProgramCommand(int ProgramId, int UserId) : IRequest<Result>;
    public record PublishProgramCommand(int ProgramId, int UserId) : IRequest<Result<ProgramDto>>;
}

namespace AlNady.Application.Features.Programs.Queries
{
    public record GetProgramByIdQuery(int ProgramId) : IRequest<Result<ProgramDto>>;
    public record SearchProgramsQuery(string? Sport, string? Location, string? AgeCategory, string? Gender, decimal? MinPrice, decimal? MaxPrice, DateTime? DateFrom, DateTime? DateTo, string? Status, int Page, int PageSize) : IRequest<Result<PagedResult<ProgramDto>>>;
    public record GetMyProgramsQuery(int UserId, int Page, int PageSize) : IRequest<Result<PagedResult<ProgramDto>>>;
}

namespace AlNady.Application.Features.Programs.Handlers
{
    public class CreateProgramCommandHandler : IRequestHandler<Commands.CreateProgramCommand, Result<ProgramDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuditService _audit;

        public CreateProgramCommandHandler(IApplicationDbContext context, IAuditService audit)
        { _context = context; _audit = audit; }

        public async Task<Result<ProgramDto>> Handle(Commands.CreateProgramCommand request, CancellationToken ct)
        {
            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == request.UserId, ct);
            var academy = await _context.Academies.FirstOrDefaultAsync(a => a.UserId == request.UserId, ct);

            if (trainer == null && academy == null)
                return Result<ProgramDto>.Failure("You must have a trainer or academy profile to create programs.");

            var program = new TrainingProgram
            {
                TrainerId = trainer?.TrainerId, AcademyId = academy?.AcademyId,
                Title = request.Title, Description = request.Description, SportType = request.SportType,
                ProgramDate = request.ProgramDate, ProgramTime = request.ProgramTime,
                Price = request.Price, Capacity = request.Capacity, AvailableSlots = request.Capacity,
                TrainingLocation = request.TrainingLocation, AgeCategory = request.AgeCategory,
                GenderPreference = request.GenderPreference, Status = ProgramStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };
            _context.TrainingPrograms.Add(program);
            await _context.SaveChangesAsync(ct);
            await _audit.LogAsync(request.UserId, EventType.ProgramCreated, $"Program '{program.Title}' created");
            var ownerName = trainer != null
                ? (await _context.Users.FindAsync(new object[] { trainer.UserId }, ct))?.FullName
                : (await _context.Users.FindAsync(new object[] { academy!.UserId }, ct))?.FullName;
            return Result<ProgramDto>.Success(MapToDto(program, ownerName, 0), 201);
        }

        internal static ProgramDto MapToDto(TrainingProgram p, string? ownerName, decimal avgRating) =>
            new(p.ProgramId, p.TrainerId, p.AcademyId, ownerName,
                p.Title, p.Description, p.SportType, p.ProgramDate, p.ProgramTime, p.Price,
                p.Status.ToString(), p.Capacity, p.AvailableSlots,
                p.TrainingLocation, p.AgeCategory, p.GenderPreference, avgRating, p.CreatedAt);
    }

    public class SearchProgramsQueryHandler : IRequestHandler<Queries.SearchProgramsQuery, Result<PagedResult<ProgramDto>>>
    {
        private readonly IApplicationDbContext _context;
        public SearchProgramsQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result<PagedResult<ProgramDto>>> Handle(Queries.SearchProgramsQuery request, CancellationToken ct)
        {
            var query = _context.TrainingPrograms.AsNoTracking()
                .Include(p => p.Trainer).ThenInclude(t => t!.User)
                .Include(p => p.Academy).ThenInclude(a => a!.User)
                .Where(p => p.Status == ProgramStatus.Published || p.Status == ProgramStatus.Ongoing)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Sport)) query = query.Where(p => p.SportType != null && p.SportType.Contains(request.Sport));
            if (!string.IsNullOrWhiteSpace(request.Location)) query = query.Where(p => p.TrainingLocation != null && p.TrainingLocation.Contains(request.Location));
            if (!string.IsNullOrWhiteSpace(request.AgeCategory)) query = query.Where(p => p.AgeCategory == request.AgeCategory);
            if (!string.IsNullOrWhiteSpace(request.Gender)) query = query.Where(p => p.GenderPreference == request.Gender);
            if (request.MinPrice.HasValue) query = query.Where(p => p.Price >= request.MinPrice.Value);
            if (request.MaxPrice.HasValue) query = query.Where(p => p.Price <= request.MaxPrice.Value);
            if (request.DateFrom.HasValue) query = query.Where(p => p.ProgramDate >= request.DateFrom.Value);
            if (request.DateTo.HasValue) query = query.Where(p => p.ProgramDate <= request.DateTo.Value);

            var total = await query.CountAsync(ct);
            var items = await query.OrderBy(p => p.ProgramDate)
                .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(p => CreateProgramCommandHandler.MapToDto(p, p.Trainer?.User?.FullName ?? p.Academy?.User?.FullName,
                p.Ratings?.Count > 0 ? (decimal)p.Ratings.Average(r => r.RatingValue) : 0)).ToList();
            return Result<PagedResult<ProgramDto>>.Success(PagedResult<ProgramDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class GetProgramByIdQueryHandler : IRequestHandler<Queries.GetProgramByIdQuery, Result<ProgramDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetProgramByIdQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result<ProgramDto>> Handle(Queries.GetProgramByIdQuery request, CancellationToken ct)
        {
            var p = await _context.TrainingPrograms.AsNoTracking()
                .Include(x => x.Trainer).ThenInclude(t => t!.User)
                .Include(x => x.Academy).ThenInclude(a => a!.User)
                .Include(x => x.Ratings)
                .FirstOrDefaultAsync(x => x.ProgramId == request.ProgramId, ct);
            if (p == null) return Result<ProgramDto>.NotFound("Program not found.");
            var avgRating = p.Ratings?.Count > 0 ? (decimal)p.Ratings.Average(r => r.RatingValue) : 0;
            return Result<ProgramDto>.Success(CreateProgramCommandHandler.MapToDto(p, p.Trainer?.User?.FullName ?? p.Academy?.User?.FullName, avgRating));
        }
    }

    public class DeleteProgramCommandHandler : IRequestHandler<Commands.DeleteProgramCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        public DeleteProgramCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result> Handle(Commands.DeleteProgramCommand request, CancellationToken ct)
        {
            var program = await _context.TrainingPrograms.Include(p => p.Trainer).Include(p => p.Academy)
                .FirstOrDefaultAsync(p => p.ProgramId == request.ProgramId, ct);
            if (program == null) return Result.NotFound("Program not found.");
            var isOwner = (program.Trainer?.UserId == request.UserId) || (program.Academy?.UserId == request.UserId);
            var isAdmin = await _context.Users.AnyAsync(u => u.UserId == request.UserId && u.Role == UserRole.Admin, ct);
            if (!isOwner && !isAdmin) return Result.Failure("You don't have permission.", 403);
            if (program.Status == ProgramStatus.Ongoing) return Result.Failure("Cannot delete an ongoing program.");
            program.Status = ProgramStatus.Cancelled;
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
    }

    public class GetMyProgramsQueryHandler : IRequestHandler<Queries.GetMyProgramsQuery, Result<PagedResult<ProgramDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetMyProgramsQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result<PagedResult<ProgramDto>>> Handle(Queries.GetMyProgramsQuery request, CancellationToken ct)
        {
            var query = _context.TrainingPrograms.AsNoTracking()
                .Include(p => p.Trainer).ThenInclude(t => t!.User)
                .Include(p => p.Academy).ThenInclude(a => a!.User)
                .Include(p => p.Ratings)
                .Where(p => (p.Trainer != null && p.Trainer.UserId == request.UserId) ||
                            (p.Academy != null && p.Academy.UserId == request.UserId));
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(p => p.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(p => CreateProgramCommandHandler.MapToDto(p,
                p.Trainer?.User?.FullName ?? p.Academy?.User?.FullName,
                p.Ratings?.Count > 0 ? (decimal)p.Ratings.Average(r => r.RatingValue) : 0)).ToList();
            return Result<PagedResult<ProgramDto>>.Success(PagedResult<ProgramDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class PublishProgramCommandHandler : IRequestHandler<Commands.PublishProgramCommand, Result<ProgramDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuditService _audit;

        public PublishProgramCommandHandler(IApplicationDbContext context, IAuditService audit)
        { _context = context; _audit = audit; }

        public async Task<Result<ProgramDto>> Handle(Commands.PublishProgramCommand request, CancellationToken ct)
        {
            var program = await _context.TrainingPrograms
                .Include(p => p.Trainer).ThenInclude(t => t!.User)
                .Include(p => p.Academy).ThenInclude(a => a!.User)
                .FirstOrDefaultAsync(p => p.ProgramId == request.ProgramId, ct);
            if (program == null) return Result<ProgramDto>.NotFound("Program not found.");
            var isOwner = (program.Trainer?.UserId == request.UserId) || (program.Academy?.UserId == request.UserId);
            if (!isOwner) return Result<ProgramDto>.Forbidden("You don't own this program.");
            if (program.Status != ProgramStatus.Draft) return Result<ProgramDto>.Failure("Only draft programs can be published.");
            var hasForm = await _context.Forms.AnyAsync(f => f.ProgramId == request.ProgramId, ct);
            if (!hasForm) return Result<ProgramDto>.Failure("You must create an enrollment form before publishing.");
            program.Status = ProgramStatus.Published;
            await _context.SaveChangesAsync(ct);
            await _audit.LogAsync(request.UserId, EventType.ProgramPublished, $"Program '{program.Title}' published");
            return Result<ProgramDto>.Success(CreateProgramCommandHandler.MapToDto(program,
                program.Trainer?.User?.FullName ?? program.Academy?.User?.FullName, 0));
        }
    }

    public class UpdateProgramCommandHandler : IRequestHandler<Commands.UpdateProgramCommand, Result<ProgramDto>>
    {
        private readonly IApplicationDbContext _context;
        public UpdateProgramCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result<ProgramDto>> Handle(Commands.UpdateProgramCommand request, CancellationToken ct)
        {
            var program = await _context.TrainingPrograms.Include(p => p.Trainer).Include(p => p.Academy)
                .FirstOrDefaultAsync(p => p.ProgramId == request.ProgramId, ct);
            if (program == null) return Result<ProgramDto>.NotFound("Program not found.");
            var isOwner = (program.Trainer?.UserId == request.UserId) || (program.Academy?.UserId == request.UserId);
            var isAdmin = await _context.Users.AnyAsync(u => u.UserId == request.UserId && u.Role == UserRole.Admin, ct);
            if (!isOwner && !isAdmin) return Result<ProgramDto>.Forbidden("Access denied.");
            if (program.Status == ProgramStatus.Ongoing || program.Status == ProgramStatus.Completed)
                return Result<ProgramDto>.Failure("Cannot update an ongoing or completed program.");
            program.Title = request.Title; program.Description = request.Description;
            program.ProgramDate = request.ProgramDate; program.ProgramTime = request.ProgramTime;
            program.Price = request.Price; program.Capacity = request.Capacity;
            program.AvailableSlots = request.Capacity; program.TrainingLocation = request.TrainingLocation;
            if (request.Status != ProgramStatus.Ongoing && request.Status != ProgramStatus.Completed)
                program.Status = request.Status;
            await _context.SaveChangesAsync(ct);
            return Result<ProgramDto>.Success(CreateProgramCommandHandler.MapToDto(program, null, 0));
        }
    }
}
