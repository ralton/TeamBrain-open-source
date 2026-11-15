using MediatR;

namespace TeamBrain.Application.LearningRecords.Commands.DeleteLearningRecord;

public record DeleteLearningRecordCommand(Guid Id) : IRequest;
