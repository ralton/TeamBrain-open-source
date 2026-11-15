using MediatR;

namespace TeamBrain.Application.Query.Commands.ExecuteQuery;

public record ExecuteQueryCommand(
    string Question,
    string? RepoId,
    string? BranchName,
    string? FilePath,
    string? Language,
    string? Framework,
    string? AdoWorkItemId,
    string? Domain,
    bool IncludeLegacyContext = false
) : IRequest<QueryResult>;

public record QueryResult(
    string AnswerMarkdown,
    string Status,
    string Confidence,
    IReadOnlyList<SourceReference> Sources,
    IReadOnlyList<string> SuggestedTests,
    IReadOnlyList<string> Warnings,
    Guid QueryLogId
);

public record SourceReference(
    string Type,
    string Id,
    string Title,
    string Status,
    string? Classification,
    string? Url
);
