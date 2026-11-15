namespace TeamBrain.Application.Common.Interfaces;

public interface IAiService
{
    Task<AiQueryResponse> QueryAsync(AiQueryRequest request, CancellationToken cancellationToken = default);
    Task<AiDraftResponse> DraftLearningRecordAsync(string threadSummary, IEnumerable<string> sourceContents, CancellationToken cancellationToken = default);
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<string[]> ExtractTagsAsync(string content, CancellationToken cancellationToken = default);
}

public record AiQueryRequest(
    string Question,
    IReadOnlyList<RetrievedContext> RetrievedContexts,
    string? RepoId,
    string? Language,
    string? Framework
);

public record RetrievedContext(
    string Id,
    string Type,
    string Title,
    string Content,
    string Status,
    string Classification
);

public record AiQueryResponse(
    string AnswerMarkdown,
    string AnswerStatus,
    string Confidence,
    string[] Warnings,
    string[] SuggestedTests,
    string ModelProvider,
    string ModelName,
    string PromptVersion
);

public record AiDraftResponse(
    string Title,
    string Summary,
    string BodyMarkdown,
    string[] Tags,
    string SuggestedCategory,
    string SuggestedDomain
);
