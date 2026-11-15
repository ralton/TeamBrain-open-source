using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.LearningRecords.Commands.CreateLearningRecord;
using TeamBrain.Application.LearningRecords.Commands.ReviewLearningRecord;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;
using Xunit;

namespace TeamBrain.Application.Tests.LearningRecords;

public class AutoPromotionAndPeerReviewTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly ISearchService _search;

    public AutoPromotionAndPeerReviewTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _audit = Substitute.For<IAuditService>();
        _search = Substitute.For<ISearchService>();

        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsAuthenticated.Returns(true);
    }

    [Fact]
    public async Task Create_HighConfidenceRecord_AutoPromotesToInReview()
    {
        var records = new List<LearningRecord>();
        var dbSet = records.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);

        var handler = new CreateLearningRecordHandler(_db, _currentUser, _audit);

        var cmd = new CreateLearningRecordCommand(
            "High Confidence Architecture Standard",
            "Summary of pattern",
            "Detailed Markdown",
            "Architecture",
            "Core",
            ConfidenceLevel.High,
            new[] { "architecture" },
            null, null, null
        );

        await handler.Handle(cmd, CancellationToken.None);

        _db.LearningRecords.Received(1).Add(Arg.Is<LearningRecord>(r =>
            r.Title == "High Confidence Architecture Standard" &&
            r.Status == KnowledgeStatus.InReview &&
            r.IsAutoPromoted == true &&
            r.AutoPromotionReason != null
        ));
    }

    [Fact]
    public async Task Review_SmeApproval_ImmediatelyApprovesRecord()
    {
        _currentUser.IsInRole("Reviewer").Returns(true);

        var recordId = Guid.NewGuid();
        var record = new LearningRecord
        {
            Id = recordId,
            Title = "Candidate Pattern",
            Status = KnowledgeStatus.InReview,
            RequiredPeerApprovals = 2
        };

        var records = new List<LearningRecord> { record };
        var dbSet = records.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);

        var reviews = new List<Review>();
        var reviewDbSet = reviews.AsQueryable().BuildMockDbSet();
        _db.Reviews.Returns(reviewDbSet);

        var handler = new ReviewLearningRecordHandler(_db, _currentUser, _audit, null, _search);

        var cmd = new ReviewLearningRecordCommand(
            recordId,
            ReviewDecision.Approve,
            "SME LGTM",
            null, null, null, null, null
        );

        await handler.Handle(cmd, CancellationToken.None);

        Assert.Equal(KnowledgeStatus.Approved, record.Status);
        Assert.NotNull(record.ApprovedAt);
    }

    [Fact]
    public async Task Review_PeerApprovals_RequiresThresholdToApprove()
    {
        _currentUser.IsInRole("Reviewer").Returns(false);
        _currentUser.IsInRole("Admin").Returns(false);
        _currentUser.IsInRole("Contributor").Returns(true);

        var recordId = Guid.NewGuid();
        var record = new LearningRecord
        {
            Id = recordId,
            Title = "Peer Review Pattern",
            Status = KnowledgeStatus.InReview,
            RequiredPeerApprovals = 2
        };

        // First peer review already recorded
        record.Reviews.Add(new Review
        {
            LearningRecordId = recordId,
            ReviewerUserId = Guid.NewGuid(),
            Decision = ReviewDecision.Approve,
            Comments = "Peer 1 LGTM"
        });

        var records = new List<LearningRecord> { record };
        var dbSet = records.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);

        var reviews = record.Reviews.ToList();
        var reviewDbSet = reviews.AsQueryable().BuildMockDbSet();
        _db.Reviews.Returns(reviewDbSet);

        var handler = new ReviewLearningRecordHandler(_db, _currentUser, _audit, null, _search);

        var cmd = new ReviewLearningRecordCommand(
            recordId,
            ReviewDecision.Approve,
            "Peer 2 LGTM — meeting threshold",
            null, null, null, null, null
        );

        await handler.Handle(cmd, CancellationToken.None);

        Assert.Equal(KnowledgeStatus.Approved, record.Status);
        Assert.Contains("Auto-approved via 2 peer review approvals", record.AutoPromotionReason);
    }
}
