using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.Feedback.Commands.RecordFeedback;
using TeamBrain.Domain.Entities;
using Xunit;

namespace TeamBrain.Application.Tests.Feedback;

public class RecordFeedbackHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly RecordFeedbackHandler _handler;

    public RecordFeedbackHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _audit = Substitute.For<IAuditService>();

        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsAuthenticated.Returns(true);

        var feedbackSet = new List<FeedbackEvent>().AsQueryable().BuildMockDbSet();
        _db.FeedbackEvents.Returns(feedbackSet);

        _handler = new RecordFeedbackHandler(_db, _currentUser, _audit);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesFeedbackEventAndLogsAudit()
    {
        var queryLogId = Guid.NewGuid();
        var command = new RecordFeedbackCommand(queryLogId, true, "Great answer!");

        var feedbackId = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, feedbackId);
        _db.FeedbackEvents.Received(1).Add(Arg.Is<FeedbackEvent>(f =>
            f.Id == feedbackId &&
            f.QueryLogId == queryLogId &&
            f.IsHelpful == true &&
            f.Comment == "Great answer!" &&
            f.UserId == _currentUser.UserId
        ));

        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _audit.Received(1).LogAsync(
            "FeedbackSubmitted",
            "FeedbackEvent",
            feedbackId.ToString(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        );
    }
}
