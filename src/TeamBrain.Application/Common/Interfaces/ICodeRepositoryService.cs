namespace TeamBrain.Application.Common.Interfaces;

public interface ICodeRepositoryService
{
    Task<RepositoryFileResult?> GetFileContentAsync(string owner, string repo, string path, string branch = "main", CancellationToken ct = default);
    Task<PullRequestDetailsResult?> GetPullRequestAsync(string owner, string repo, int prNumber, CancellationToken ct = default);
    Task<RepositoryInfoResult?> GetRepositoryAsync(string owner, string repo, CancellationToken ct = default);
}

public record RepositoryFileResult(
    string Path,
    string Content,
    string Sha,
    long Size
);

public record PullRequestDetailsResult(
    int Number,
    string Title,
    string State,
    string Author,
    string HtmlUrl,
    string HeadBranch,
    string BaseBranch,
    DateTime CreatedAt,
    DateTime? MergedAt
);

public record RepositoryInfoResult(
    string Name,
    string Owner,
    string DefaultBranch,
    string HtmlUrl,
    bool IsPrivate
);
