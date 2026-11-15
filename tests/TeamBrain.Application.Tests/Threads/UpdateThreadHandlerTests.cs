using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.Threads.Commands.UpdateThread;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;
using Xunit;

namespace TeamBrain.Application.Tests.Threads;

public class UpdateThreadHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly UpdateThreadHandler _handler;

    public UpdateThreadHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _audit = Substitute.For<IAuditService>();
        _handler = new UpdateThreadHandler(_db, _audit);
    }

    [Fact]
    public async Task Handle_ExistingThread_UpdatesFieldsAndLogsAudit()
    {
        var threadId = Guid.NewGuid();
        var thread = new TeamBrainThread
        {
            Id = threadId,
            Title = "Original Title",
            ThreadType = ThreadType.Investigation,
            Status = "active",
            IsDeleted = false
        };

        var dbSet = new List<TeamBrainThread> { thread }.AsQueryable().BuildMockDbSet();
        _db.Threads.Returns(dbSet);

        var command = new UpdateThreadCommand(
            threadId,
            Title: "Updated Title",
            Status: "resolved",
            RepoId: "RepoA",
            AdoWorkItemId: "999"
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.True(result);
        Assert.Equal("Updated Title", thread.Title);
        Assert.Equal("resolved", thread.Status);
        Assert.Equal("RepoA", thread.RepoId);
        Assert.Equal("999", thread.AdoWorkItemId);

        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _audit.Received(1).LogAsync(
            "ThreadUpdated",
            "TeamBrainThread",
            threadId.ToString(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_NonExistentThread_ReturnsFalse()
    {
        var dbSet = new List<TeamBrainThread>().AsQueryable().BuildMockDbSet();
        _db.Threads.Returns(dbSet);

        var command = new UpdateThreadCommand(Guid.NewGuid(), Title: "Non-existent");
        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.False(result);
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
