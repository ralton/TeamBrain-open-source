using MockQueryable.NSubstitute;
using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.Ingestion.Commands.IndexLearningRecord;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;
using Xunit;

namespace TeamBrain.Infrastructure.Tests.Ingestion;

public class IndexLearningRecordHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ISearchService _search;
    private readonly IndexLearningRecordHandler _handler;

    public IndexLearningRecordHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _search = Substitute.For<ISearchService>();

        _handler = new IndexLearningRecordHandler(_db, _search);
    }

    [Fact]
    public async Task Handle_RecordNotFound_ThrowsKeyNotFoundException()
    {
        _db.LearningRecords.Returns(new List<LearningRecord>().AsQueryable().BuildMockDbSet());

        var cmd = new IndexLearningRecordCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_RecordNotApproved_ThrowsInvalidOperationException()
    {
        var recordId = Guid.NewGuid();
        var record = new LearningRecord
        {
            Id = recordId,
            Title = "Candidate Pattern",
            Status = KnowledgeStatus.Candidate
        };

        _db.LearningRecords.Returns(new List<LearningRecord> { record }.AsQueryable().BuildMockDbSet());

        var cmd = new IndexLearningRecordCommand(recordId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ApprovedRecord_IndexesDocument()
    {
        var recordId = Guid.NewGuid();
        var record = new LearningRecord
        {
            Id = recordId,
            Title = "Approved Pattern",
            Summary = "Approved pattern summary",
            Category = "Architecture",
            Status = KnowledgeStatus.Approved,
            Confidence = ConfidenceLevel.High
        };

        _db.LearningRecords.Returns(new List<LearningRecord> { record }.AsQueryable().BuildMockDbSet());

        var cmd = new IndexLearningRecordCommand(recordId);

        await _handler.Handle(cmd, CancellationToken.None);

        await _search.Received(1).IndexLearningRecordAsync(Arg.Is<SearchDocument>(doc =>
            doc.LearningRecordId == recordId.ToString() &&
            doc.Title == "Approved Pattern" &&
            doc.Status == KnowledgeStatus.Approved
        ), Arg.Any<CancellationToken>());
    }
}
