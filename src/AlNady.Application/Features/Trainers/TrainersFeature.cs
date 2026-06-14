using AlNady.Application.Features.Trainers.DTOs;
using AlNady.Application.Interfaces;
using AlNady.Domain.Entities;
using AlNady.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlNady.Application.Features.Trainers.Commands
{
    public record CreateTrainerProfileCommand(int UserId, string? About, string? SpecializationSports, string? AgeCategory, string? GenderPreference) : IRequest<Result<TrainerProfileDto>>;
    public record UpdateTrainerProfileCommand(int UserId, string? About, string? SpecializationSports, string? AgeCategory, string? GenderPreference) : IRequest<Result<TrainerProfileDto>>;
    public record UploadCertificateCommand(int UserId, Stream FileStream, string FileName, string ContentType, string CertificateName) : IRequest<Result<CertificateDto>>;
}

namespace AlNady.Application.Features.Trainers.Queries
{
    public record GetTrainerByIdQuery(int TrainerId) : IRequest<Result<TrainerProfileDto>>;
    public record GetTrainerByUserIdQuery(int UserId) : IRequest<Result<TrainerProfileDto>>;
    public record GetTrainersQuery(int Page, int PageSize, string? Sport, string? AgeCategory, string? Gender, bool? VerifiedOnly) : IRequest<Result<PagedResult<TrainerProfileDto>>>;
}

namespace AlNady.Application.Features.Trainers.Handlers
{
    using AlNady.Application.Features.Trainers.Commands;
    using AlNady.Application.Features.Trainers.Queries;
    public class CreateTrainerProfileHandler : IRequestHandler<CreateTrainerProfileCommand, Result<TrainerProfileDto>>
    {
        private readonly IApplicationDbContext _context;
        public CreateTrainerProfileHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result<TrainerProfileDto>> Handle(CreateTrainerProfileCommand request, CancellationToken ct)
        {
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, ct);
            if (user == null) return Result<TrainerProfileDto>.NotFound("User not found.");

            if (await _context.Trainers.AnyAsync(t => t.UserId == request.UserId, ct))
                return Result<TrainerProfileDto>.Conflict("Trainer profile already exists.");

            var trainer = new Trainer
            {
                UserId = request.UserId,
                About = request.About,
                SpecializationSports = request.SpecializationSports,
                AgeCategory = request.AgeCategory,
                GenderPreference = request.GenderPreference,
                IsVerifiedByAdmin = false,
                AverageRating = 0
            };

            _context.Trainers.Add(trainer);
            await _context.SaveChangesAsync(ct);

            return Result<TrainerProfileDto>.Success(new TrainerProfileDto(
                trainer.TrainerId, trainer.UserId, user.FullName, user.Email, user.ProfileImage,
                trainer.About, trainer.SpecializationSports, trainer.IsVerifiedByAdmin, trainer.AverageRating,
                trainer.AgeCategory, trainer.GenderPreference, new()), 201);
        }
    }

    public class GetTrainersQueryHandler : IRequestHandler<GetTrainersQuery, Result<PagedResult<TrainerProfileDto>>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileStorageService _storage;

        public GetTrainersQueryHandler(IApplicationDbContext context, IFileStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        public async Task<Result<PagedResult<TrainerProfileDto>>> Handle(GetTrainersQuery request, CancellationToken ct)
        {
            var query = _context.Trainers.AsNoTracking()
                .Include(t => t.User).Include(t => t.Certificates).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Sport))
                query = query.Where(t => t.SpecializationSports != null && t.SpecializationSports.Contains(request.Sport));
            if (!string.IsNullOrWhiteSpace(request.AgeCategory))
                query = query.Where(t => t.AgeCategory == request.AgeCategory);
            if (!string.IsNullOrWhiteSpace(request.Gender))
                query = query.Where(t => t.GenderPreference == request.Gender);
            if (request.VerifiedOnly == true)
                query = query.Where(t => t.IsVerifiedByAdmin);

            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(t => t.AverageRating)
                .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);

            var dtos = items.Select(t => new TrainerProfileDto(
                t.TrainerId, t.UserId, t.User.FullName, t.User.Email, t.User.ProfileImage,
                t.About, t.SpecializationSports, t.IsVerifiedByAdmin, t.AverageRating,
                t.AgeCategory, t.GenderPreference,
                t.Certificates.Select(c => new CertificateDto(c.CertificateId, c.CertificateName,
                    _storage.GetPublicUrl(c.FilePath), c.IsVerifiedByAdmin, c.DateAdded)).ToList()
            )).ToList();

            return Result<PagedResult<TrainerProfileDto>>.Success(
                PagedResult<TrainerProfileDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class UploadCertificateCommandHandler : IRequestHandler<UploadCertificateCommand, Result<CertificateDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IFileStorageService _storage;

        public UploadCertificateCommandHandler(IApplicationDbContext context, IFileStorageService storage)
        {
            _context = context;
            _storage = storage;
        }

        public async Task<Result<CertificateDto>> Handle(UploadCertificateCommand request, CancellationToken ct)
        {
            var trainer = await _context.Trainers.FirstOrDefaultAsync(t => t.UserId == request.UserId, ct);
            if (trainer == null) return Result<CertificateDto>.NotFound("Trainer profile not found.");

            var allowed = new[] { "application/pdf", "image/jpeg", "image/png" };
            if (!_storage.IsValidFileType(request.ContentType, allowed))
                return Result<CertificateDto>.Failure("Only PDF, JPEG, PNG files allowed.");
            if (!_storage.IsValidFileSize(request.FileStream.Length, 10 * 1024 * 1024))
                return Result<CertificateDto>.Failure("File must not exceed 10MB.");

            var filePath = await _storage.UploadAsync(request.FileStream, request.FileName, request.ContentType, "certificates", ct);

            var cert = new Certificate
            {
                TrainerId = trainer.TrainerId,
                CertificateName = request.CertificateName,
                FilePath = filePath,
                IsVerifiedByAdmin = false,
                DateAdded = DateTime.UtcNow
            };

            _context.Certificates.Add(cert);
            await _context.SaveChangesAsync(ct);

            return Result<CertificateDto>.Success(new CertificateDto(
                cert.CertificateId, cert.CertificateName, _storage.GetPublicUrl(filePath), false, cert.DateAdded), 201);
        }
    }
}
