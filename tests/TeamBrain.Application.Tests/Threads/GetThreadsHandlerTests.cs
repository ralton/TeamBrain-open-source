using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.Threads.Queries.GetThreads;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;
using Xunit;

namespace TeamBrain.Application.Tests.Threads;

public class GetThreadsHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly GetThreadsHandler _handler;

    public GetThreadsHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();

        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsAuthenticated.Returns(true);

        _handler = new GetThreadsHandler(_db, _currentUser);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyUserThreads()
    {
        var currentUserId = _currentUser.UserId;
        var thread1 = new TeamBrainThread
        {
            Id = Guid.NewGuid(),
            Title = "Thread 1",
            CreatedByUserId = currentUserId,
            ThreadType = ThreadType.Investigation,
            Status = "active",
            IsDeleted = false,
            UpdatedAt = DateTime.UtcNow
        };
        var thread2 = new TeamBrainThread
        {
            Id = Guid.NewGuid(),
            Title = "Thread 2",
            CreatedByUserId = Guid.NewGuid(),
            ThreadType = ThreadType.Feature,
            Status = "active",
            IsDeleted = false,
            UpdatedAt = DateTime.UtcNow
        };

        var threads = new List<TeamBrainThread> { thread1, thread2 };
        var dbSet = threads.AsQueryable().BuildMockDbSet();
        _db.Threads.Returns(dbSet);

        var query = new GetThreadsQuery(1, 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Thread 1", result.Items[0].Title);
    }
}
