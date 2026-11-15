using MediatR;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.LearningRecords.Queries.SearchKnowledgeRecords;

public record SearchKnowledgeRecordsQuery(
    string? SearchText = null,
    string? Category = null,
    string? Domain = null,
    KnowledgeStatus? Status = null,
    ConfidenceLevel? MinimumConfidence = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<SearchKnowledgeRecordsResult>;

public record SearchKnowledgeRecordsResult(
    IReadOnlyList<KnowledgeRecordSearchItem> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record KnowledgeRecordSearchItem(
    Guid Id,
    string Title,
    string Summary,
    string? BodyMarkdown,
    string Category,
    string? Domain,
    KnowledgeStatus Status,
    ConfidenceLevel Confidence,
    int Version,
    bool IsAutoPromoted,
    string[] Tags,
    string? ExternalWorkItemId,
    string? ExternalWorkItemProvider,
    string? ExternalWorkItemUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    double Score = 1.0
);
