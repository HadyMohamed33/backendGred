using AlNady.Domain.Enums;

namespace AlNady.Application.Features.Programs.DTOs;

public record ProgramDto(
    int ProgramId,
    int? TrainerId,
    int? AcademyId,
    string? OwnerName,
    string? Title,
    string? Description,
    string? SportType,
    DateTime ProgramDate,
    TimeSpan ProgramTime,
    decimal Price,
    string Status,
    int Capacity,
    int AvailableSlots,
    string? TrainingLocation,
    string? AgeCategory,
    string? GenderPreference,
    decimal AverageRating,
    DateTime CreatedAt
);

public record CreateProgramRequest(
    string? Title,
    string? Description,
    string? SportType,
    DateTime ProgramDate,
    TimeSpan ProgramTime,
    decimal Price,
    int Capacity,
    string? TrainingLocation,
    string? AgeCategory,
    string? GenderPreference
);

public record SearchProgramsRequest(
    string? Sport,
    string? Location,
    string? AgeCategory,
    string? Gender,
    decimal? MinPrice,
    decimal? MaxPrice,
    DateTime? DateFrom,
    DateTime? DateTo,
    string? Status,
    int Page = 1,
    int PageSize = 10
);
