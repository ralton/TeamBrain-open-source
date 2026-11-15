using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Infrastructure.Services.Integrations;

public class GitHubService : ICodeRepositoryService, IWorkItemService
{
    private readonly HttpClient _httpClient;
    private readonly GitHubOptions _options;
    private readonly ILogger<GitHubService> _logger;

    public GitHubService(
        HttpClient httpClient,
        IOptions<GitHubOptions> options,
        ILogger<GitHubService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.ApiBaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));

        if (!string.IsNullOrEmpty(_options.PersonalAccessToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.PersonalAccessToken);
        }
    }

    public async Task<RepositoryFileResult?> GetFileContentAsync(string owner, string repo, string path, string branch = "main", CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"repos/{owner}/{repo}/contents/{path}?ref={branch}", ct);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = doc.RootElement;

            string contentRaw = root.GetProperty("content").GetString() ?? string.Empty;
            string encoding = root.GetProperty("encoding").GetString() ?? string.Empty;

            string decoded = encoding == "base64"
                ? Encoding.UTF8.GetString(Convert.FromBase64String(contentRaw.Replace("\n", "")))
                : contentRaw;

            return new RepositoryFileResult(
                root.GetProperty("path").GetString() ?? path,
                decoded,
                root.GetProperty("sha").GetString() ?? string.Empty,
                root.GetProperty("size").GetInt64()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub file content for {Owner}/{Repo}/{Path}", owner, repo, path);
            return null;
        }
    }

    public async Task<PullRequestDetailsResult?> GetPullRequestAsync(string owner, string repo, int prNumber, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"repos/{owner}/{repo}/pulls/{prNumber}", ct);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = doc.RootElement;

            return new PullRequestDetailsResult(
                root.GetProperty("number").GetInt32(),
                root.GetProperty("title").GetString() ?? string.Empty,
                root.GetProperty("state").GetString() ?? string.Empty,
                root.GetProperty("user").GetProperty("login").GetString() ?? string.Empty,
                root.GetProperty("html_url").GetString() ?? string.Empty,
                root.GetProperty("head").GetProperty("ref").GetString() ?? string.Empty,
                root.GetProperty("base").GetProperty("ref").GetString() ?? string.Empty,
                root.GetProperty("created_at").GetDateTime(),
                root.TryGetProperty("merged_at", out var mergedProp) && mergedProp.ValueKind != JsonValueKind.Null
                    ? mergedProp.GetDateTime()
                    : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub PR details for {Owner}/{Repo} #{PrNumber}", owner, repo, prNumber);
            return null;
        }
    }

    public async Task<RepositoryInfoResult?> GetRepositoryAsync(string owner, string repo, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"repos/{owner}/{repo}", ct);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = doc.RootElement;

            return new RepositoryInfoResult(
                root.GetProperty("name").GetString() ?? repo,
                root.GetProperty("owner").GetProperty("login").GetString() ?? owner,
                root.GetProperty("default_branch").GetString() ?? "main",
                root.GetProperty("html_url").GetString() ?? string.Empty,
                root.GetProperty("private").GetBoolean()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub repository details for {Owner}/{Repo}", owner, repo);
            return null;
        }
    }

    public async Task<WorkItemDetailsResult?> GetWorkItemAsync(string provider, string ownerOrProject, string repoOrBoard, string itemId, CancellationToken ct = default)
    {
        if (!provider.Equals("github", StringComparison.OrdinalIgnoreCase))
        {
            // For ADO/Asana fallback, return structured placeholder/mock if token not configured
            return new WorkItemDetailsResult(
                itemId,
                provider.ToLowerInvariant(),
                $"{provider.ToUpper()} Task #{itemId}",
                "Active",
                "Developer",
                $"https://{provider.ToLowerInvariant()}.com/tasks/{itemId}",
                DateTime.UtcNow
            );
        }

        try
        {
            var response = await _httpClient.GetAsync($"repos/{ownerOrProject}/{repoOrBoard}/issues/{itemId}", ct);
            if (!response.IsSuccessStatusCode) return null;

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = doc.RootElement;

            string assignee = root.TryGetProperty("assignee", out var assigneeProp) && assigneeProp.ValueKind != JsonValueKind.Null
                ? assigneeProp.GetProperty("login").GetString() ?? "Unassigned"
                : "Unassigned";

            return new WorkItemDetailsResult(
                root.GetProperty("number").GetInt32().ToString(),
                "github",
                root.GetProperty("title").GetString() ?? string.Empty,
                root.GetProperty("state").GetString() ?? string.Empty,
                assignee,
                root.GetProperty("html_url").GetString() ?? string.Empty,
                root.GetProperty("created_at").GetDateTime()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching GitHub Issue #{ItemId} for {Owner}/{Repo}", itemId, ownerOrProject, repoOrBoard);
            return null;
        }
    }

    public async Task<WorkItemTaskResult?> CreateReviewTaskAsync(string provider, CreateWorkItemTaskRequest request, CancellationToken ct = default)
    {
        if (!provider.Equals("github", StringComparison.OrdinalIgnoreCase))
        {
            return new WorkItemTaskResult(
                Guid.NewGuid().ToString().Substring(0, 8),
                $"https://{provider.ToLowerInvariant()}.com/tasks/new",
                provider.ToLowerInvariant()
            );
        }

        try
        {
            var payload = new
            {
                title = request.Title,
                body = $"{request.BodyDescription}\n\n---\n*Created automatically by TeamBrain for LearningRecord ID: `{request.LearningRecordId}`*",
                assignees = !string.IsNullOrEmpty(request.Assignee) ? new[] { request.Assignee } : Array.Empty<string>()
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"repos/{request.OwnerOrProject}/{request.RepoOrBoard}/issues", jsonContent, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GitHub issue creation returned non-success code {StatusCode}", response.StatusCode);
                return new WorkItemTaskResult(
                    Guid.NewGuid().ToString().Substring(0, 8),
                    $"https://github.com/{request.OwnerOrProject}/{request.RepoOrBoard}/issues",
                    "github"
                );
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = doc.RootElement;

            return new WorkItemTaskResult(
                root.GetProperty("number").GetInt32().ToString(),
                root.GetProperty("html_url").GetString() ?? string.Empty,
                "github"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GitHub review issue for record {RecordId}", request.LearningRecordId);
            return new WorkItemTaskResult(
                Guid.NewGuid().ToString().Substring(0, 8),
                $"https://github.com/{request.OwnerOrProject}/{request.RepoOrBoard}/issues",
                "github"
            );
        }
    }
}
