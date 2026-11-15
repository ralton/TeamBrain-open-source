using MediatR;

namespace TeamBrain.Application.Ingestion.Commands.IndexLearningRecord;

public record IndexLearningRecordCommand(Guid LearningRecordId) : IRequest;
