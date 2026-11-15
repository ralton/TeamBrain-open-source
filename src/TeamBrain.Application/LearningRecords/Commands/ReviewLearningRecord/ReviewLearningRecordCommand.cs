using MediatR;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.LearningRecords.Commands.ReviewLearningRecord;

public record ReviewLearningRecordCommand(
    Guid LearningRecordId,
    ReviewDecision Decision,
    string? Comments,
    string? UpdatedTitle,
    string? UpdatedSummary,
    string? UpdatedBodyMarkdown,
    ConfidenceLevel? UpdatedConfidence,
    DateTime? ExpiresAt
) : IRequest;
