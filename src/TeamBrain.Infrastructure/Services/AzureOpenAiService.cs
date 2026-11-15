using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System.Text.Json;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Infrastructure.Services;

public class AzureOpenAiOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string ChatDeployment { get; set; } = string.Empty;
    public string EmbeddingDeployment { get; set; } = string.Empty;
    public string PromptVersion { get; set; } = "v1";
}

public class AzureOpenAiService(
    AzureOpenAIClient client,
    IOptions<AzureOpenAiOptions> options
) : IAiService
{
    private static readonly string SystemPrompt = """
        You are TeamBrain, an engineering knowledge assistant.
        You answer questions about approved team patterns, standards, and implementations.
        Rules:
        - Only use the provided context. Do not invent facts, file names, services, or APIs.
        - Clearly state whether a pattern is Approved, Legacy, Deprecated, or Unknown.
        - Always cite the source IDs from the provided context.
        - If no approved pattern exists, say so explicitly. Do not guess.
        - Never expose secrets, credentials, or connection strings.
        - Respond in concise engineering prose. Use markdown.
        """;

    public async Task<AiQueryResponse> QueryAsync(AiQueryRequest request, CancellationToken cancellationToken = default)
    {
        var contextBlock = BuildContextBlock(request.RetrievedContexts);
        var jsonSchema = """
            {
              "answer_markdown": "string",
              "answer_status": "approved_pattern_found|no_approved_pattern|partial_match",
              "confidence": "high|medium|low",
              "warnings": ["string"],
              "suggested_tests": ["string"]
            }
            """;
        var userMessage = $"""
            Context:
            {contextBlock}

            Question: {request.Question}
            Repo: {request.RepoId ?? "unspecified"}
            Language: {request.Language ?? "unspecified"}
            Framework: {request.Framework ?? "unspecified"}

            Respond with JSON matching this schema:
            {jsonSchema}
            """;

        var chatClient = client.GetChatClient(options.Value.ChatDeployment);
        var messages = new ChatMessage[]
        {
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(userMessage)
        };

        var completion = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        var json = completion.Value.Content[0].Text;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new AiQueryResponse(
                root.GetProperty("answer_markdown").GetString() ?? string.Empty,
                root.GetProperty("answer_status").GetString() ?? "unknown",
                root.GetProperty("confidence").GetString() ?? "low",
                root.TryGetProperty("warnings", out var w) ? w.EnumerateArray().Select(e => e.GetString()!).ToArray() : Array.Empty<string>(),
                root.TryGetProperty("suggested_tests", out var t) ? t.EnumerateArray().Select(e => e.GetString()!).ToArray() : Array.Empty<string>(),
                "azure_openai",
                options.Value.ChatDeployment,
                options.Value.PromptVersion
            );
        }
        catch
        {
            return new AiQueryResponse(
                json,
                "unknown",
                "low",
                Array.Empty<string>(),
                Array.Empty<string>(),
                "azure_openai",
                options.Value.ChatDeployment,
                options.Value.PromptVersion
            );
        }
    }

    public async Task<AiDraftResponse> DraftLearningRecordAsync(string threadSummary, IEnumerable<string> sourceContents, CancellationToken cancellationToken = default)
    {
        var draftSchema = """
            {
              "title": "string",
              "summary": "string",
              "body_markdown": "string",
              "tags": ["string"],
              "suggested_category": "string",
              "suggested_domain": "string"
            }
            """;
        var prompt = $"""
            Based on this engineering thread, draft a Learning Record.
            Thread: {threadSummary}
            Sources: {string.Join(", ", sourceContents)}

            Respond with JSON:
            {draftSchema}
            """;

        var chatClient = client.GetChatClient(options.Value.ChatDeployment);
        var completion = await chatClient.CompleteChatAsync(
            new[] { new UserChatMessage(prompt) }, cancellationToken: cancellationToken);

        var json = completion.Value.Content[0].Text;
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new AiDraftResponse(
            root.GetProperty("title").GetString() ?? "Draft Learning Record",
            root.GetProperty("summary").GetString() ?? string.Empty,
            root.GetProperty("body_markdown").GetString() ?? string.Empty,
            root.TryGetProperty("tags", out var tags) ? tags.EnumerateArray().Select(e => e.GetString()!).ToArray() : Array.Empty<string>(),
            root.GetProperty("suggested_category").GetString() ?? "Engineering Standard",
            root.GetProperty("suggested_domain").GetString() ?? string.Empty
        );
    }

    public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var embeddingClient = client.GetEmbeddingClient(options.Value.EmbeddingDeployment);
        var response = await embeddingClient.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return response.Value.ToFloats().ToArray();
    }

    public async Task<string[]> ExtractTagsAsync(string content, CancellationToken cancellationToken = default)
    {
        var prompt = $"Extract 5-10 relevant technical tags from this content as a JSON string array: {content[..Math.Min(content.Length, 2000)]}";
        var chatClient = client.GetChatClient(options.Value.ChatDeployment);
        var completion = await chatClient.CompleteChatAsync(
            new[] { new UserChatMessage(prompt) }, cancellationToken: cancellationToken);
        var json = completion.Value.Content[0].Text;
        return JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>();
    }

    private static string BuildContextBlock(IReadOnlyList<RetrievedContext> contexts) =>
        string.Join("\n---\n", contexts.Select(c =>
            $"[{c.Type}] {c.Title} (Status: {c.Status}, Classification: {c.Classification})\n{c.Content}"));
}
