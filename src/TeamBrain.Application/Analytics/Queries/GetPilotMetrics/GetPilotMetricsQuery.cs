using MediatR;

namespace TeamBrain.Application.Analytics.Queries.GetPilotMetrics;

public record GetPilotMetricsQuery() : IRequest<PilotMetricsDto>;

public record PilotMetricsDto(
    int TotalLearningRecords,
    int ApprovedRecords,
    int CandidateRecords,
    int LegacyRecords,
    int AntiPatternRecords,
    int TotalQueriesAsked,
    int ApprovedPatternMatches,
    double PatternMatchRatePercentage,
    int TotalFeedbackCount,
    int HelpfulCount,
    int NotHelpfulCount,
    double SentimentHelpfulPercentage,
    List<RecentFeedbackDto> RecentFeedbackComments
);

public record RecentFeedbackDto(
    Guid FeedbackId,
    Guid QueryLogId,
    bool IsHelpful,
    string? Comment,
    DateTime CreatedAt
);
