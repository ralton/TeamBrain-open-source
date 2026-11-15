using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.Analytics.Queries.GetPilotMetrics;

public class GetPilotMetricsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPilotMetricsQuery, PilotMetricsDto>
{
    public async Task<PilotMetricsDto> Handle(GetPilotMetricsQuery request, CancellationToken cancellationToken)
    {
        var records = await dbContext.LearningRecords
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .ToListAsync(cancellationToken);

        var totalRecords = records.Count;
        var approvedRecords = records.Count(r => r.Status == KnowledgeStatus.Approved);
        var candidateRecords = records.Count(r => r.Status == KnowledgeStatus.Candidate);
        var legacyRecords = records.Count(r => r.Status == KnowledgeStatus.Legacy);
        var antiPatternRecords = records.Count(r => r.Category.Contains("Anti", StringComparison.OrdinalIgnoreCase) || r.Status.ToString().Contains("Anti", StringComparison.OrdinalIgnoreCase));

        var queryLogs = await dbContext.QueryLogs.AsNoTracking().ToListAsync(cancellationToken);
        var totalQueries = queryLogs.Count;
        var approvedMatches = queryLogs.Count(q => q.AnswerStatus == "approved_pattern_found");
        var matchRate = totalQueries > 0 ? Math.Round((double)approvedMatches / totalQueries * 100.0, 1) : 0.0;

        var feedbacks = await dbContext.FeedbackEvents.AsNoTracking().ToListAsync(cancellationToken);
        var totalFeedback = feedbacks.Count;
        var helpfulCount = feedbacks.Count(f => f.IsHelpful);
        var notHelpfulCount = feedbacks.Count(f => !f.IsHelpful);
        var sentimentHelpfulPct = totalFeedback > 0 ? Math.Round((double)helpfulCount / totalFeedback * 100.0, 1) : 0.0;

        var recentComments = feedbacks
            .Where(f => !string.IsNullOrEmpty(f.Comment))
            .OrderByDescending(f => f.CreatedAt)
            .Take(10)
            .Select(f => new RecentFeedbackDto(
                f.Id,
                f.QueryLogId,
                f.IsHelpful,
                f.Comment,
                f.CreatedAt
            ))
            .ToList();

        return new PilotMetricsDto(
            totalRecords,
            approvedRecords,
            candidateRecords,
            legacyRecords,
            antiPatternRecords,
            totalQueries,
            approvedMatches,
            matchRate,
            totalFeedback,
            helpfulCount,
            notHelpfulCount,
            sentimentHelpfulPct,
            recentComments
        );
    }
}
