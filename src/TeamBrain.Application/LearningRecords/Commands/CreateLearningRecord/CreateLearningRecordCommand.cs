using MediatR;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.LearningRecords.Commands.CreateLearningRecord;

public record CreateLearningRecordCommand(
    string Title,
    string Summary,
    string? BodyMarkdown,
    string Category,
    string? Domain,
    ConfidenceLevel Confidence,
    string[] Tags,
    Guid? OwnerTeamId,
    Guid? OwnerUserId,
    Guid? ThreadId
) : IRequest<Guid>;
