using MediatR;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.Query.Commands.ExecuteQuery;

public class ExecuteQueryHandler(
    ISearchService search,
    IAiService ai,
    IApplicationDbContext db,
    ICurrentUserService currentUser
) : IRequestHandler<ExecuteQueryCommand, QueryResult>
{
    public async Task<QueryResult> Handle(ExecuteQueryCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        var statuses = request.IncludeLegacyContext
            ? new[] { KnowledgeStatus.Approved, KnowledgeStatus.Legacy }
            : new[] { KnowledgeStatus.Approved };

        var searchQuery = new SearchQuery(
            Text: request.Question,
            RepoId: request.RepoId,
            Language: request.Language,
            Domain: request.Domain,
            Category: null,
            Statuses: statuses,
            Team: null,
            IncludeLegacy: request.IncludeLegacyContext,
            Top: 8
        );

        var searchResults = await search.SearchAsync(searchQuery, cancellationToken);

        var contexts = searchResults.Results.Select(r => new RetrievedContext(
            r.Id,
            r.RecordType,
            r.Title,
            r.Summary,
            r.Status.ToString(),
            r.Classification.ToString()
        )).ToList();

        var aiRequest = new AiQueryRequest(
            request.Question,
            contexts,
            request.RepoId,
            request.Language,
            request.Framework
        );

        var aiResponse = await ai.QueryAsync(aiRequest, cancellationToken);
        var latencyMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

        var sources = searchResults.Results.Select(r => new SourceReference(
            r.RecordType,
            r.LearningRecordId ?? r.ArtifactId ?? r.Id,
            r.Title,
            r.Status.ToString(),
            r.Classification.ToString(),
            r.SourceUrl
        )).ToList();

        var log = new QueryLog
        {
            UserId = currentUser.IsAuthenticated ? currentUser.UserId : null,
            QuestionText = request.Question,
            RepoId = request.RepoId,
            Domain = request.Domain,
            AnswerStatus = aiResponse.AnswerStatus,
            Confidence = aiResponse.Confidence,
            LearningRecordIds = string.Join(",", sources.Where(s => s.Type == "learning_record").Select(s => s.Id)),
            SourceArtifactIds = string.Join(",", sources.Where(s => s.Type != "learning_record").Select(s => s.Id)),
            ModelProvider = aiResponse.ModelProvider,
            ModelName = aiResponse.ModelName,
            PromptVersion = aiResponse.PromptVersion,
            LatencyMs = latencyMs
        };
        db.QueryLogs.Add(log);
        await db.SaveChangesAsync(cancellationToken);

        return new QueryResult(
            aiResponse.AnswerMarkdown,
            aiResponse.AnswerStatus,
            aiResponse.Confidence,
            sources,
            aiResponse.SuggestedTests,
            aiResponse.Warnings,
            log.Id
        );
    }
}
