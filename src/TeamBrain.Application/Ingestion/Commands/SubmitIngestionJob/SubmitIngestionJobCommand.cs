using MediatR;

namespace TeamBrain.Application.Ingestion.Commands.SubmitIngestionJob;

public record SubmitIngestionJobCommand(
    string SourceType,
    string SourceReference
) : IRequest<Guid>;
