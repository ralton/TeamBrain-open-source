using MediatR;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.LearningRecords.Commands.UpdateLearningRecord;

public record UpdateLearningRecordCommand(
    Guid Id,
    string? Title,
    string? Summary,
    string? BodyMarkdown,
    string? Category,
    string? Domain,
    ConfidenceLevel? Confidence,
    string[]? Tags
) : IRequest;
