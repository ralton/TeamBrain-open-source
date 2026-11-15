using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Enums;
using AppSearchDocument = TeamBrain.Application.Common.Interfaces.SearchDocument;
using AzureSearchDocument = Azure.Search.Documents.Models.SearchDocument;

namespace TeamBrain.Infrastructure.Services;

public class AzureAiSearchOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string IndexName { get; set; } = "teambrain-knowledge-v1";
}

public class AzureAiSearchService(
    SearchClient searchClient,
    IOptions<AzureAiSearchOptions> options
) : ISearchService
{
    public async Task IndexLearningRecordAsync(AppSearchDocument document, CancellationToken cancellationToken = default)
    {
        var doc = MapToSearchDocument(document);
        await searchClient.MergeOrUploadDocumentsAsync(new[] { doc }, cancellationToken: cancellationToken);
    }

    public async Task IndexSourceArtifactAsync(AppSearchDocument document, CancellationToken cancellationToken = default)
    {
        var doc = MapToSearchDocument(document);
        await searchClient.MergeOrUploadDocumentsAsync(new[] { doc }, cancellationToken: cancellationToken);
    }

    public async Task<SearchResults> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(query);

        var searchOptions = new SearchOptions
        {
            Filter = filter,
            Size = query.Top,
            Skip = query.Skip,
            IncludeTotalCount = true,
            Select = { "id", "record_type", "learning_record_id", "artifact_id", "title", "summary",
                       "status", "classification", "confidence", "source_url" }
        };

        var response = await searchClient.SearchAsync<Azure.Search.Documents.Models.SearchDocument>(
            query.Text, searchOptions, cancellationToken);

        var results = new List<SearchResult>();
        await foreach (var result in response.Value.GetResultsAsync())
        {
            var doc = (AzureSearchDocument)result.Document;
            results.Add(new SearchResult(
                doc.GetString("id") ?? string.Empty,
                doc.GetString("record_type") ?? string.Empty,
                doc.GetString("learning_record_id"),
                doc.GetString("artifact_id"),
                doc.GetString("title") ?? string.Empty,
                doc.GetString("summary") ?? string.Empty,
                ParseStatus(doc.GetString("status")),
                ParseClassification(doc.GetString("classification")),
                ParseConfidence(doc.GetString("confidence")),
                doc.GetString("source_url"),
                result.Score ?? 0
            ));
        }

        return new SearchResults(results, response.Value.TotalCount ?? 0);
    }

    public async Task DeleteDocumentAsync(string id, CancellationToken cancellationToken = default)
    {
        await searchClient.DeleteDocumentsAsync("id", new[] { id }, cancellationToken: cancellationToken);
    }

    private static string BuildFilter(SearchQuery query)
    {
        var filters = new List<string>();

        if (query.Statuses?.Length > 0)
        {
            var statusValues = string.Join(",", query.Statuses.Select(s => $"'{s.ToString().ToLower()}'"));
            filters.Add($"search.in(status, {statusValues})");
        }

        if (!string.IsNullOrWhiteSpace(query.RepoId))
            filters.Add($"repo_id eq '{query.RepoId}'");

        if (!string.IsNullOrWhiteSpace(query.Domain))
            filters.Add($"domain eq '{query.Domain}'");

        if (!string.IsNullOrWhiteSpace(query.Category))
            filters.Add($"category eq '{query.Category}'");

        return string.Join(" and ", filters);
    }

    private static AzureSearchDocument MapToSearchDocument(AppSearchDocument doc)
    {
        var searchDoc = new AzureSearchDocument
        {
            ["id"] = doc.Id,
            ["record_type"] = doc.RecordType,
            ["learning_record_id"] = doc.LearningRecordId,
            ["artifact_id"] = doc.ArtifactId,
            ["title"] = doc.Title,
            ["content"] = doc.Content,
            ["summary"] = doc.Summary,
            ["category"] = doc.Category,
            ["domain"] = doc.Domain,
            ["systems"] = doc.Systems,
            ["languages"] = doc.Languages,
            ["repo_id"] = doc.RepoId,
            ["file_path"] = doc.FilePath,
            ["status"] = doc.Status.ToString().ToLower(),
            ["classification"] = doc.Classification.ToString().ToLower(),
            ["confidence"] = doc.Confidence.ToString().ToLower(),
            ["tags"] = doc.Tags,
            ["owner_team"] = doc.OwnerTeam,
            ["approved_at"] = doc.ApprovedAt,
            ["reviewed_at"] = doc.ReviewedAt,
            ["expires_at"] = doc.ExpiresAt,
            ["source_url"] = doc.SourceUrl
        };
        return searchDoc;
    }

    private static KnowledgeStatus ParseStatus(string? s) =>
        Enum.TryParse<KnowledgeStatus>(s, true, out var v) ? v : KnowledgeStatus.Candidate;

    private static ArtifactClassification ParseClassification(string? s) =>
        Enum.TryParse<ArtifactClassification>(s, true, out var v) ? v : ArtifactClassification.Unknown;

    private static ConfidenceLevel ParseConfidence(string? s) =>
        Enum.TryParse<ConfidenceLevel>(s, true, out var v) ? v : ConfidenceLevel.Medium;
}

internal static class SearchDocumentExtensions
{
    internal static string? GetString(this AzureSearchDocument doc, string key) =>
        doc.TryGetValue(key, out var val) ? val?.ToString() : null;
}
