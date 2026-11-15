using MediatR;

namespace TeamBrain.Application.Ingestion.Queries.GetIngestionJobs;

public record GetIngestionJobsQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<IngestionJobsListVm>;

public record IngestionJobsListVm(
    List<IngestionJobDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record IngestionJobDto(
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
    DateTime? CompletedAt
);
