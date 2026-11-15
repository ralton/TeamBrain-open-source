using MediatR;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.LearningRecords.Queries.GetLearningRecords;

public record GetLearningRecordsQuery(
    KnowledgeStatus? Status,
    string? Category,
    string? Domain,
    string? Search,
    int Page = 1,
    int PageSize = 20
) : IRequest<GetLearningRecordsResult>;

public record GetLearningRecordsResult(IReadOnlyList<LearningRecordSummary> Items, int TotalCount);

public record LearningRecordSummary(
    Guid Id,
    string Title,
    string Summary,
    string Category,
    string? Domain,
    KnowledgeStatus Status,
    ConfidenceLevel Confidence,
    string[] Tags,
    DateTime? ApprovedAt,
    DateTime? ReviewedAt,
    DateTime UpdatedAt
);
