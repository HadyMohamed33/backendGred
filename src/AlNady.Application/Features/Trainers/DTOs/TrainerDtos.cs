namespace AlNady.Application.Features.Trainers.DTOs;

public record TrainerProfileDto(
    int TrainerId,
    int UserId,
    string FullName,
    string Email,
    string? ProfileImage,
    string? About,
    string? SpecializationSports,
    bool IsVerifiedByAdmin,
    decimal AverageRating,
    string? AgeCategory,
    string? GenderPreference,
    List<CertificateDto> Certificates
);

public record CertificateDto(
    int CertificateId,
    string CertificateName,
    string FileUrl,
    bool IsVerifiedByAdmin,
    DateTime DateAdded
);

public record CreateTrainerProfileRequest(
    string? About,
    string? SpecializationSports,
    string? AgeCategory,
    string? GenderPreference
);
