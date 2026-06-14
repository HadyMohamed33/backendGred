namespace AlNady.Application.Features.Academies.DTOs;

public record AcademyProfileDto(
    int AcademyId,
    int UserId,
    string FullName,
    string Email,
    string? ProfileImage,
    string? SpecializationSports,
    bool IsVerified,
    decimal AverageRating,
    string? Location,
    string? AgeCategory,
    string? GenderPreference,
    List<AlNady.Application.Features.Trainers.DTOs.CertificateDto> Certificates
);

public record CreateAcademyProfileRequest(
    string? SpecializationSports,
    string? Location,
    string? AgeCategory,
    string? GenderPreference
);
