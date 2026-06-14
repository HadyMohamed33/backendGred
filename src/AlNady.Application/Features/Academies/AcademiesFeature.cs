using AlNady.Application.Features.Academies.DTOs;
using AlNady.Application.Features.Trainers.DTOs;
using AlNady.Application.Interfaces;
using AlNady.Domain.Entities;
using AlNady.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AlNady.Application.Features.Academies.Commands
{
    public record CreateAcademyProfileCommand(int UserId, string? SpecializationSports, string? Location, string? AgeCategory, string? GenderPreference) : IRequest<Result<AcademyProfileDto>>;
    public record UpdateAcademyProfileCommand(int UserId, string? SpecializationSports, string? Location, string? AgeCategory, string? GenderPreference) : IRequest<Result<AcademyProfileDto>>;
    public record UploadAcademyCertificateCommand(int UserId, Stream FileStream, string FileName, string ContentType, string CertificateName) : IRequest<Result<CertificateDto>>;
}

namespace AlNady.Application.Features.Academies.Queries
{
    public record GetAcademiesQuery(int Page, int PageSize, string? Sport, string? Location, bool? VerifiedOnly) : IRequest<Result<PagedResult<AcademyProfileDto>>>;
    public record GetAcademyByIdQuery(int AcademyId) : IRequest<Result<AcademyProfileDto>>;
    public record GetAcademyByUserIdQuery(int UserId) : IRequest<Result<AcademyProfileDto>>;
}

namespace AlNady.Application.Features.Academies.Handlers
{
    public class CreateAcademyProfileHandler : IRequestHandler<Commands.CreateAcademyProfileCommand, Result<AcademyProfileDto>>
    {
        private readonly IApplicationDbContext _context;
        public CreateAcademyProfileHandler(IApplicationDbContext context) => _context = context;

        public async Task<Result<AcademyProfileDto>> Handle(Commands.CreateAcademyProfileCommand request, CancellationToken ct)
        {
            var user = await _context.Users.FindAsync(new object[] { request.UserId }, ct);
            if (user == null) return Result<AcademyProfileDto>.NotFound("User not found.");
            if (await _context.Academies.AnyAsync(a => a.UserId == request.UserId, ct))
                return Result<AcademyProfileDto>.Conflict("Academy profile already exists.");
            var academy = new Academy { UserId = request.UserId, SpecializationSports = request.SpecializationSports, Location = request.Location, AgeCategory = request.AgeCategory, GenderPreference = request.GenderPreference, IsVerified = false, AverageRating = 0 };
            _context.Academies.Add(academy);
            await _context.SaveChangesAsync(ct);
            return Result<AcademyProfileDto>.Success(new AcademyProfileDto(academy.AcademyId, academy.UserId, user.FullName, user.Email, user.ProfileImage, academy.SpecializationSports, academy.IsVerified, academy.AverageRating, academy.Location, academy.AgeCategory, academy.GenderPreference, new()), 201);
        }
    }

    public class UpdateAcademyProfileHandler : IRequestHandler<Commands.UpdateAcademyProfileCommand, Result<AcademyProfileDto>>
    {
        private readonly IApplicationDbContext _context; private readonly IFileStorageService _storage;
        public UpdateAcademyProfileHandler(IApplicationDbContext context, IFileStorageService storage) { _context = context; _storage = storage; }

        public async Task<Result<AcademyProfileDto>> Handle(Commands.UpdateAcademyProfileCommand request, CancellationToken ct)
        {
            var academy = await _context.Academies.Include(a => a.User).Include(a => a.Certificates).FirstOrDefaultAsync(a => a.UserId == request.UserId, ct);
            if (academy == null) return Result<AcademyProfileDto>.NotFound("Academy profile not found.");
            academy.SpecializationSports = request.SpecializationSports; academy.Location = request.Location; academy.AgeCategory = request.AgeCategory; academy.GenderPreference = request.GenderPreference;
            await _context.SaveChangesAsync(ct);
            return Result<AcademyProfileDto>.Success(new AcademyProfileDto(academy.AcademyId, academy.UserId, academy.User.FullName, academy.User.Email, academy.User.ProfileImage, academy.SpecializationSports, academy.IsVerified, academy.AverageRating, academy.Location, academy.AgeCategory, academy.GenderPreference,
                academy.Certificates.Select(c => new CertificateDto(c.CertificateId, c.CertificateName, _storage.GetPublicUrl(c.FilePath), c.IsVerifiedByAdmin, c.DateAdded)).ToList()));
        }
    }

    public class UploadAcademyCertificateCommandHandler : IRequestHandler<Commands.UploadAcademyCertificateCommand, Result<CertificateDto>>
    {
        private readonly IApplicationDbContext _context; private readonly IFileStorageService _storage;
        public UploadAcademyCertificateCommandHandler(IApplicationDbContext context, IFileStorageService storage) { _context = context; _storage = storage; }
        public async Task<Result<CertificateDto>> Handle(Commands.UploadAcademyCertificateCommand request, CancellationToken ct)
        {
            var academy = await _context.Academies.FirstOrDefaultAsync(a => a.UserId == request.UserId, ct);
            if (academy == null) return Result<CertificateDto>.NotFound("Academy profile not found.");
            var allowed = new[] { "application/pdf", "image/jpeg", "image/png" };
            if (!_storage.IsValidFileType(request.ContentType, allowed)) return Result<CertificateDto>.Failure("Only PDF, JPEG, PNG files allowed.");
            if (!_storage.IsValidFileSize(request.FileStream.Length, 10 * 1024 * 1024)) return Result<CertificateDto>.Failure("File must not exceed 10MB.");
            var filePath = await _storage.UploadAsync(request.FileStream, request.FileName, request.ContentType, "certificates", ct);
            var cert = new Certificate { AcademyId = academy.AcademyId, CertificateName = request.CertificateName, FilePath = filePath, IsVerifiedByAdmin = false, DateAdded = DateTime.UtcNow };
            _context.Certificates.Add(cert);
            await _context.SaveChangesAsync(ct);
            return Result<CertificateDto>.Success(new CertificateDto(cert.CertificateId, cert.CertificateName, _storage.GetPublicUrl(filePath), false, cert.DateAdded), 201);
        }
    }

    public class GetAcademiesQueryHandler : IRequestHandler<Queries.GetAcademiesQuery, Result<PagedResult<AcademyProfileDto>>>
    {
        private readonly IApplicationDbContext _context; private readonly IFileStorageService _storage;
        public GetAcademiesQueryHandler(IApplicationDbContext context, IFileStorageService storage) { _context = context; _storage = storage; }
        public async Task<Result<PagedResult<AcademyProfileDto>>> Handle(Queries.GetAcademiesQuery request, CancellationToken ct)
        {
            var query = _context.Academies.AsNoTracking().Include(a => a.User).Include(a => a.Certificates).AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Sport)) query = query.Where(a => a.SpecializationSports != null && a.SpecializationSports.Contains(request.Sport));
            if (!string.IsNullOrWhiteSpace(request.Location)) query = query.Where(a => a.Location != null && a.Location.Contains(request.Location));
            if (request.VerifiedOnly == true) query = query.Where(a => a.IsVerified);
            var total = await query.CountAsync(ct);
            var items = await query.OrderByDescending(a => a.AverageRating).Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
            var dtos = items.Select(a => new AcademyProfileDto(a.AcademyId, a.UserId, a.User.FullName, a.User.Email, a.User.ProfileImage, a.SpecializationSports, a.IsVerified, a.AverageRating, a.Location, a.AgeCategory, a.GenderPreference,
                a.Certificates.Select(c => new CertificateDto(c.CertificateId, c.CertificateName, _storage.GetPublicUrl(c.FilePath), c.IsVerifiedByAdmin, c.DateAdded)).ToList())).ToList();
            return Result<PagedResult<AcademyProfileDto>>.Success(PagedResult<AcademyProfileDto>.Create(dtos, total, request.Page, request.PageSize));
        }
    }

    public class GetAcademyByIdQueryHandler : IRequestHandler<Queries.GetAcademyByIdQuery, Result<AcademyProfileDto>>
    {
        private readonly IApplicationDbContext _context; private readonly IFileStorageService _storage;
        public GetAcademyByIdQueryHandler(IApplicationDbContext context, IFileStorageService storage) { _context = context; _storage = storage; }
        public async Task<Result<AcademyProfileDto>> Handle(Queries.GetAcademyByIdQuery request, CancellationToken ct)
        {
            var a = await _context.Academies.AsNoTracking().Include(x => x.User).Include(x => x.Certificates).FirstOrDefaultAsync(x => x.AcademyId == request.AcademyId, ct);
            if (a == null) return Result<AcademyProfileDto>.NotFound("Academy not found.");
            return Result<AcademyProfileDto>.Success(new AcademyProfileDto(a.AcademyId, a.UserId, a.User.FullName, a.User.Email, a.User.ProfileImage, a.SpecializationSports, a.IsVerified, a.AverageRating, a.Location, a.AgeCategory, a.GenderPreference,
                a.Certificates.Select(c => new CertificateDto(c.CertificateId, c.CertificateName, _storage.GetPublicUrl(c.FilePath), c.IsVerifiedByAdmin, c.DateAdded)).ToList()));
        }
    }

    public class GetAcademyByUserIdQueryHandler : IRequestHandler<Queries.GetAcademyByUserIdQuery, Result<AcademyProfileDto>>
    {
        private readonly IApplicationDbContext _context; private readonly IFileStorageService _storage;
        public GetAcademyByUserIdQueryHandler(IApplicationDbContext context, IFileStorageService storage) { _context = context; _storage = storage; }
        public async Task<Result<AcademyProfileDto>> Handle(Queries.GetAcademyByUserIdQuery request, CancellationToken ct)
        {
            var a = await _context.Academies.AsNoTracking().Include(x => x.User).Include(x => x.Certificates).FirstOrDefaultAsync(x => x.UserId == request.UserId, ct);
            if (a == null) return Result<AcademyProfileDto>.NotFound("Academy profile not found.");
            return Result<AcademyProfileDto>.Success(new AcademyProfileDto(a.AcademyId, a.UserId, a.User.FullName, a.User.Email, a.User.ProfileImage, a.SpecializationSports, a.IsVerified, a.AverageRating, a.Location, a.AgeCategory, a.GenderPreference,
                a.Certificates.Select(c => new CertificateDto(c.CertificateId, c.CertificateName, _storage.GetPublicUrl(c.FilePath), c.IsVerifiedByAdmin, c.DateAdded)).ToList()));
        }
    }
}
