using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.LearningRecords.Commands.DeleteLearningRecord;
using TeamBrain.Domain.Entities;
using Xunit;

namespace TeamBrain.Application.Tests.LearningRecords;

public class DeleteLearningRecordHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly DeleteLearningRecordHandler _handler;

    public DeleteLearningRecordHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _audit = Substitute.For<IAuditService>();
        _handler = new DeleteLearningRecordHandler(_db, _audit);
    }

    [Fact]
    public async Task Handle_RecordNotFound_ThrowsKeyNotFoundException()
    {
        var records = new List<LearningRecord>();
        var dbSet = records.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);

        var cmd = new DeleteLearningRecordCommand(Guid.NewGuid());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidRecord_SetsIsDeletedTrue()
    {
        var recordId = Guid.NewGuid();
        var record = new LearningRecord
        {
            Id = recordId,
            Title = "Test Pattern",
            IsDeleted = false
        };

        var records = new List<LearningRecord> { record };
        var dbSet = records.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);

        var cmd = new DeleteLearningRecordCommand(recordId);

        await _handler.Handle(cmd, CancellationToken.None);

        Assert.True(record.IsDeleted);
        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
