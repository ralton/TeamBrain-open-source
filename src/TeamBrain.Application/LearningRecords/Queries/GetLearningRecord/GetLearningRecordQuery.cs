using MediatR;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.LearningRecords.Queries.GetLearningRecord;

public record GetLearningRecordQuery(Guid Id) : IRequest<LearningRecordDetail?>;

public record LearningRecordDetail(
    Guid Id,
    string Title,
    string Summary,
    string? BodyMarkdown,
    string Category,
    string? Domain,
    KnowledgeStatus Status,
    ConfidenceLevel Confidence,
    int Version,
    string[] Tags,
    Guid? OwnerTeamId,
    Guid? OwnerUserId,
    Guid CreatedByUserId,
    Guid? ApprovedByUserId,
    DateTime? ReviewedAt,
    DateTime? ApprovedAt,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<SourceSummary> Sources,
    IReadOnlyList<ReviewSummary> ReviewHistory
);

public record SourceSummary(Guid Id, string Title, string ArtifactType, string Classification, string? SourceUrl);
public record ReviewSummary(Guid ReviewerId, string Decision, string? Comments, DateTime CreatedAt);
