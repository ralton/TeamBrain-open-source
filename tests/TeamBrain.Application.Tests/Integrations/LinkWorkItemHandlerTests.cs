using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.Integrations.Commands.LinkWorkItem;
using TeamBrain.Domain.Entities;
using Xunit;

namespace TeamBrain.Application.Tests.Integrations;

public class LinkWorkItemHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly LinkWorkItemHandler _handler;

    public LinkWorkItemHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _audit = Substitute.For<IAuditService>();
        _handler = new LinkWorkItemHandler(_db, _audit);
    }

    [Fact]
    public async Task Handle_ThreadTarget_UpdatesExternalWorkItemAndLogsAudit()
    {
        var threadId = Guid.NewGuid();
        var thread = new TeamBrainThread
        {
            Id = threadId,
            Title = "Engineering Thread",
            IsDeleted = false
        };

        var dbSet = new List<TeamBrainThread> { thread }.AsQueryable().BuildMockDbSet();
        _db.Threads.Returns(dbSet);

        var command = new LinkWorkItemCommand(
            threadId,
            "thread",
            "github",
            "42",
            "https://github.com/my-org/my-repo/issues/42"
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        Assert.Equal("42", thread.ExternalWorkItemId);
        Assert.Equal("github", thread.ExternalWorkItemProvider);
        Assert.Equal("https://github.com/my-org/my-repo/issues/42", thread.ExternalWorkItemUrl);

        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _audit.Received(1).LogAsync(
            "WorkItemLinked",
            "TeamBrainThread",
            threadId.ToString(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_LearningRecordTarget_UpdatesExternalWorkItemAndLogsAudit()
    {
        var recordId = Guid.NewGuid();
        var record = new LearningRecord
        {
            Id = recordId,
            Title = "Pattern Record",
            IsDeleted = false
        };

        var dbSet = new List<LearningRecord> { record }.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);

        var command = new LinkWorkItemCommand(
            recordId,
            "learning_record",
            "github",
            "108",
            "https://github.com/my-org/my-repo/issues/108"
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        Assert.Equal("108", record.ExternalWorkItemId);
        Assert.Equal("github", record.ExternalWorkItemProvider);
        Assert.Equal("https://github.com/my-org/my-repo/issues/108", record.ExternalWorkItemUrl);

        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
