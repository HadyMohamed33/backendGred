using AlNady.Application.Interfaces;
using AlNady.Domain.Entities;
using AlNady.Domain.Enums;
using AlNady.Shared.Common;
using AlNady.Shared.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlNady.Application.Features.Ratings.DTOs
{
    public record RatingDto(int RatingId, int UserId, string UserName, int ProgramId, int RatingValue, string? Comment, List<RatingAspectDto> Aspects, DateTime CreatedAt, DateTime? UpdatedAt);
    public record RatingAspectDto(string AspectName, int RatingValue);
    public record SubmitRatingRequest(int RatingValue, string? Comment, List<RatingAspectDto> Aspects);
}

namespace AlNady.Application.Features.Ratings.Commands
{
    using DTOs;
    public record SubmitRatingCommand(int ProgramId, int UserId, int RatingValue, string? Comment, List<RatingAspectDto> Aspects) : IRequest<Result<RatingDto>>;
    public record EditRatingCommand(int RatingId, int UserId, int RatingValue, string? Comment, List<RatingAspectDto> Aspects) : IRequest<Result<RatingDto>>;
}

namespace AlNady.Application.Features.Ratings.Queries
{
    using DTOs;
    public record GetProgramRatingsQuery(int ProgramId, int Page, int PageSize) : IRequest<Result<PagedResult<RatingDto>>>;
}

namespace AlNady.Application.Features.Ratings.Handlers
{
    using DTOs;

    public class SubmitRatingCommandHandler : IRequestHandler<Commands.SubmitRatingCommand, Result<RatingDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuditService _audit;

        public SubmitRatingCommandHandler(IApplicationDbContext context, IAuditService audit)
        { _context = context; _audit = audit; }

        public async Task<Result<RatingDto>> Handle(Commands.SubmitRatingCommand request, CancellationToken ct)
        {
            if (request.RatingValue < AppConstants.Rating.MinValue || request.RatingValue > AppConstants.Rating.MaxValue)
                return Result<RatingDto>.Failure($"Rating must be between {AppConstants.Rating.MinValue} and {AppConstants.Rating.MaxValue}.");

            var program = await _context.TrainingPrograms.FirstOrDefaultAsync(p => p.ProgramId == request.ProgramId, ct);
            if (program == null) return Result<RatingDto>.NotFound("Program not found.");
            if (program.Status != ProgramStatus.Completed) return Result<RatingDto>.Failure("You can only rate completed programs.");

            var wasEnrolled = await _context.FormResponses.AnyAsync(r =>
                r.Form.ProgramId == request.ProgramId && r.UserId == request.UserId && r.Status == FormResponseStatus.Enrolled, ct);
            if (!wasEnrolled) return Result<RatingDto>.Forbidden("You must be enrolled in this program to rate it.");

            if (await _context.Ratings.AnyAsync(r => r.ProgramId == request.ProgramId && r.UserId == request.UserId, ct))
                return Result<RatingDto>.Conflict("You have already rated this program.");

            var rating = new Rating
            {
                ProgramId = request.ProgramId, UserId = request.UserId,
                RatingValue = request.RatingValue, Comment = request.Comment, CreatedAt = DateTime.UtcNow,
                Aspects = request.Aspects.Select(a => new RatingAspect { AspectName = a.AspectName, RatingValue = a.RatingValue }).ToList()
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync(ct);
            await RecalculateAverageAsync(request.ProgramId, ct);
            await _audit.LogAsync(request.UserId, EventType.RatingSubmitted, $"Rated program {request.ProgramId}: {request.RatingValue}/5");

            var user = await _context.Users.FindAsync(new object[] { request.UserId }, ct);
            return Result<RatingDto>.Success(new RatingDto(rating.RatingId, rating.UserId, user?.FullName ?? "", rating.ProgramId, rating.RatingValue, rating.Comment,
                rating.Aspects.Select(a => new RatingAspectDto(a.AspectName, a.RatingValue)).ToList(), rating.CreatedAt, null), 201);
        }

        private async Task RecalculateAverageAsync(int programId, CancellationToken ct)
        {
            var program = await _context.TrainingPrograms.FindAsync(new object[] { programId }, ct);
            if (program == null) return;
            if (program.TrainerId.HasValue)
            {
                var trainer = await _context.Trainers.FindAsync(new object[] { program.TrainerId.Value }, ct);
                if (trainer != null) trainer.AverageRating = (decimal)await _context.Ratings.Where(r => r.Program.TrainerId == program.TrainerId).AverageAsync(r => (double)r.RatingValue, ct);
            }
            else if (program.AcademyId.HasValue)
            {
                var academy = await _context.Academies.FindAsync(new object[] { program.AcademyId.Value }, ct);
                if (academy != null) academy.AverageRating = (decimal)await _context.Ratings.Where(r => r.Program.AcademyId == program.AcademyId).AverageAsync(r => (double)r.RatingValue, ct);
            }
            await _context.SaveChangesAsync(ct);
        }
    }

    public class EditRatingCommandHandler : IRequestHandler<Commands.EditRatingCommand, Result<RatingDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IAuditService _audit;

        public EditRatingCommandHandler(IApplicationDbContext context, IAuditService audit)
        { _context = context; _audit = audit; }

        public async Task<Result<RatingDto>> Handle(Commands.EditRatingCommand request, CancellationToken ct)
        {
            var rating = await _context.Ratings.Include(r => r.Aspects).FirstOrDefaultAsync(r => r.RatingId == request.RatingId, ct);
            if (rating == null) return Result<RatingDto>.NotFound("Rating not found.");
            if (rating.UserId != request.UserId) return Result<RatingDto>.Forbidden();
            if ((DateTime.UtcNow - rating.CreatedAt).TotalHours > AppConstants.Rating.EditWindowHours)
                return Result<RatingDto>.Failure("Edit window of 24 hours has passed.");

            rating.RatingValue = request.RatingValue; rating.Comment = request.Comment; rating.UpdatedAt = DateTime.UtcNow;
            _context.RatingAspects.RemoveRange(rating.Aspects);
            rating.Aspects = request.Aspects.Select(a => new RatingAspect { RatingId = rating.RatingId, AspectName = a.AspectName, RatingValue = a.RatingValue }).ToList();
            await _context.SaveChangesAsync(ct);
            await _audit.LogAsync(request.UserId, EventType.RatingEdited, $"Edited rating {request.RatingId}");

            var user = await _context.Users.FindAsync(new object[] { request.UserId }, ct);
            return Result<RatingDto>.Success(new RatingDto(rating.RatingId, rating.UserId, user?.FullName ?? "", rating.ProgramId, rating.RatingValue, rating.Comment,
                rating.Aspects.Select(a => new RatingAspectDto(a.AspectName, a.RatingValue)).ToList(), rating.CreatedAt, rating.UpdatedAt));
        }
    }

    public class GetProgramRatingsQueryHandler : IRequestHandler<Queries.GetProgramRatingsQuery, Result<PagedResult<RatingDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetProgramRatingsQueryHandler(IApplicationDbContext context) => _context = context;
        public async Task<Result<PagedResult<RatingDto>>> Handle(Queries.GetProgramRatingsQuery request, CancellationToken ct)
        {
            var query = _context.Ratings.AsNoTracking().Include(r => r.User).Include(r => r.Aspects).Where(r => r.ProgramId == request.ProgramId);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(r => r.CreatedAt).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(r => new RatingDto(r.RatingId, r.UserId, r.User.FullName, r.ProgramId, r.RatingValue, r.Comment,
                r.Aspects.Select(a => new RatingAspectDto(a.AspectName, a.RatingValue)).ToList(), r.CreatedAt, r.UpdatedAt)).ToList();
            return Result<PagedResult<RatingDto>>.Success(PagedResult<RatingDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }
}
