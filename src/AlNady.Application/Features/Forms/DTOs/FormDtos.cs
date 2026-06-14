using AlNady.Domain.Enums;

namespace AlNady.Application.Features.Forms.DTOs;

public record FormDto(
    int FormId,
    int ProgramId,
    string Title,
    List<FormFieldDto> Fields
);

public record FormFieldDto(
    int FieldId,
    string Label,
    string FieldType,
    bool IsRequired,
    int DisplayOrder,
    string? Options,
    string? Placeholder
);

public record FormResponseDto(
    int ResponseId,
    int FormId,
    int UserId,
    string UserName,
    string Status,
    DateTime SubmittedAt,
    List<FieldValueDto> FieldValues
);

public record FieldValueDto(
    int FieldId,
    string Label,
    string? Value
);

public record CreateFormRequest(
    string Title,
    List<CreateFormFieldRequest> Fields
);

public record CreateFormFieldRequest(
    string Label,
    FieldType FieldType,
    bool IsRequired,
    int DisplayOrder,
    string? Options,
    string? Placeholder
);

public record SubmitFormResponseRequest(
    List<SubmitFieldValueRequest> FieldValues
);

public record SubmitFieldValueRequest(
    int FieldId,
    string? Value
);
