using NSubstitute;
using TeamBrain.Application.Analytics.Queries.GetPilotMetrics;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;
using Xunit;

namespace TeamBrain.Application.Tests.Analytics;

public class GetPilotMetricsHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly GetPilotMetricsHandler _handler;

    public GetPilotMetricsHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new GetPilotMetricsHandler(_db);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectCalculatedMetrics()
    {
        var record1 = new LearningRecord { Id = Guid.NewGuid(), Title = "Rec 1", Status = KnowledgeStatus.Approved, Category = "Service Pattern", IsDeleted = false };
        var record2 = new LearningRecord { Id = Guid.NewGuid(), Title = "Rec 2", Status = KnowledgeStatus.Candidate, Category = "Database", IsDeleted = false };
        var recordsSet = new List<LearningRecord> { record1, record2 }.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(recordsSet);

        var queryLog1 = new QueryLog { Id = Guid.NewGuid(), QuestionText = "Q1", AnswerStatus = "approved_pattern_found" };
        var queryLogsSet = new List<QueryLog> { queryLog1 }.AsQueryable().BuildMockDbSet();
        _db.QueryLogs.Returns(queryLogsSet);

        var feedback1 = new FeedbackEvent { Id = Guid.NewGuid(), QueryLogId = queryLog1.Id, IsHelpful = true, Comment = "Helpful answer!", CreatedAt = DateTime.UtcNow };
        var feedbacksSet = new List<FeedbackEvent> { feedback1 }.AsQueryable().BuildMockDbSet();
        _db.FeedbackEvents.Returns(feedbacksSet);

        var result = await _handler.Handle(new GetPilotMetricsQuery(), CancellationToken.None);

        Assert.Equal(2, result.TotalLearningRecords);
        Assert.Equal(1, result.ApprovedRecords);
        Assert.Equal(1, result.CandidateRecords);
        Assert.Equal(1, result.TotalQueriesAsked);
        Assert.Equal(100.0, result.PatternMatchRatePercentage);
        Assert.Equal(1, result.TotalFeedbackCount);
        Assert.Equal(100.0, result.SentimentHelpfulPercentage);
        Assert.Single(result.RecentFeedbackComments);
        Assert.Equal("Helpful answer!", result.RecentFeedbackComments[0].Comment);
    }
}
