using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.Common.Interfaces;

public interface ISearchService
{
    Task IndexLearningRecordAsync(SearchDocument document, CancellationToken cancellationToken = default);
    Task IndexSourceArtifactAsync(SearchDocument document, CancellationToken cancellationToken = default);
    Task<SearchResults> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
    Task DeleteDocumentAsync(string id, CancellationToken cancellationToken = default);
}

public record SearchDocument(
    string Id,
    string RecordType,
    string? LearningRecordId,
    string? ArtifactId,
    string Title,
    string Content,
    string Summary,
    string Category,
    string? Domain,
    string[] Systems,
    string[] Languages,
    string? RepoId,
    string? FilePath,
    KnowledgeStatus Status,
    ArtifactClassification Classification,
    ConfidenceLevel Confidence,
    string[] Tags,
    string? OwnerTeam,
    DateTime? ApprovedAt,
    DateTime? ReviewedAt,
    DateTime? ExpiresAt,
    string? SourceUrl
);

public record SearchQuery(
    string? Text,
    string? RepoId,
    string? Language,
    string? Domain,
    string? Category,
    KnowledgeStatus[]? Statuses,
    string? Team,
    bool IncludeLegacy = false,
    bool IncludeCandidate = false,
    int Top = 10,
    int Skip = 0
);

public record SearchResults(IReadOnlyList<SearchResult> Results, long TotalCount);

public record SearchResult(
    string Id,
    string RecordType,
    string? LearningRecordId,
    string? ArtifactId,
    string Title,
    string Summary,
    KnowledgeStatus Status,
    ArtifactClassification Classification,
    ConfidenceLevel Confidence,
    string? SourceUrl,
    double Score
);
