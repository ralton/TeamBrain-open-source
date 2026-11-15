using MediatR;

namespace TeamBrain.Application.Ingestion.Commands.ProcessIngestionJob;

public record ProcessIngestionJobCommand(
    Guid JobId,
    string StorageUri,
    string SourceType
) : IRequest<ProcessIngestionJobResult>;

public record ProcessIngestionJobResult(
    bool Success,
    int ArtifactsProcessed,
    int ArtifactsQuarantined,
    string? ErrorMessage
);
