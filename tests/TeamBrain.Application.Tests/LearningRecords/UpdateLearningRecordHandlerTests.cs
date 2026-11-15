using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.LearningRecords.Commands.UpdateLearningRecord;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;
using Xunit;

namespace TeamBrain.Application.Tests.LearningRecords;

public class UpdateLearningRecordHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly ISearchService _search;
    private readonly UpdateLearningRecordHandler _handler;

    public UpdateLearningRecordHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _audit = Substitute.For<IAuditService>();
        _search = Substitute.For<ISearchService>();

        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsAuthenticated.Returns(true);

        _handler = new UpdateLearningRecordHandler(_db, _currentUser, _audit, _search);
    }

    [Fact]
    public async Task Handle_RecordNotFound_ThrowsKeyNotFoundException()
    {
        var records = new List<LearningRecord>();
        var dbSet = records.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);

        var cmd = new UpdateLearningRecordCommand(
            Guid.NewGuid(),
            "Updated Title",
            "Updated Summary",
            null, null, null, null, null
        );

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidRecord_UpdatesFieldsAndCreatesVersion()
    {
        var recordId = Guid.NewGuid();
        var existingRecord = new LearningRecord
        {
            Id = recordId,
            Title = "Old Title",
            Summary = "Old Summary",
            Version = 1,
            IsDeleted = false,
            Status = KnowledgeStatus.Candidate
        };

        var records = new List<LearningRecord> { existingRecord };
        var dbSet = records.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);

        var versionSnapshots = new List<LearningRecordVersion>();
        var versionDbSet = versionSnapshots.AsQueryable().BuildMockDbSet();
        _db.LearningRecordVersions.Returns(versionDbSet);

        var cmd = new UpdateLearningRecordCommand(
            recordId,
            "Updated Title",
            "Updated Summary",
            "New Body Markdown",
            "New Category",
            "New Domain",
            ConfidenceLevel.High,
            new[] { "new-tag" }
        );

        await _handler.Handle(cmd, CancellationToken.None);

        Assert.Equal("Updated Title", existingRecord.Title);
        Assert.Equal("Updated Summary", existingRecord.Summary);
        Assert.Equal(2, existingRecord.Version);
        _db.LearningRecordVersions.Received(1).Add(Arg.Is<LearningRecordVersion>(v =>
            v.LearningRecordId == recordId &&
            v.Title == "Old Title" &&
            v.Version == 1
        ));
    }
}
