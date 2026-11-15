using MediatR;

namespace TeamBrain.Application.Ingestion.Queries.GetIngestionJobById;

public record GetIngestionJobByIdQuery(Guid Id) : IRequest<IngestionJobDetailDto?>;

public record IngestionJobDetailDto(
    Guid Id,
    string SourceType,
    string SourceReference,
    string Status,
    int ArtifactsProcessed,
    int ArtifactsQuarantined,
    string? ErrorMessage,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    List<IngestionErrorDto> Errors
);

public record IngestionErrorDto(
    Guid Id,
    string ErrorType,
    string Message,
    string? SourceReference,
    DateTime CreatedAt
);
