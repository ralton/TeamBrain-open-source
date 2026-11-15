using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.Ingestion.Commands.SubmitIngestionJob;
using TeamBrain.Domain.Entities;
using Xunit;

namespace TeamBrain.Application.Tests.Ingestion;

public class SubmitIngestionJobHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IServiceBusService _serviceBus;
    private readonly SubmitIngestionJobHandler _handler;

    public SubmitIngestionJobHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _serviceBus = Substitute.For<IServiceBusService>();

        _currentUser.UserId.Returns(Guid.NewGuid());

        var jobsSet = new List<IngestionJob>().AsQueryable().BuildMockDbSet();
        _db.IngestionJobs.Returns(jobsSet);

        _handler = new SubmitIngestionJobHandler(_db, _currentUser, _serviceBus);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesJobAndSendsServiceBusMessage()
    {
        var command = new SubmitIngestionJobCommand("blob_container", "https://storage.blob.core.windows.net/container");

        var jobId = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, jobId);
        _db.IngestionJobs.Received(1).Add(Arg.Is<IngestionJob>(j =>
            j.Id == jobId &&
            j.SourceType == "blob_container" &&
            j.SourceReference == "https://storage.blob.core.windows.net/container" &&
            j.Status == "pending" &&
            j.CreatedByUserId == _currentUser.UserId
        ));

        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        await _serviceBus.Received(1).SendMessageAsync(
            "ingestion-jobs",
            Arg.Any<object>(),
            Arg.Any<CancellationToken>()
        );
    }
}
