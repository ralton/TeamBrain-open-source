using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.Ingestion.Queries.GetIngestionJobs;
using TeamBrain.Domain.Entities;
using Xunit;

namespace TeamBrain.Application.Tests.Ingestion;

public class GetIngestionJobsHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly GetIngestionJobsHandler _handler;

    public GetIngestionJobsHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _handler = new GetIngestionJobsHandler(_db);
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedIngestionJobs()
    {
        var job1 = new IngestionJob
        {
            Id = Guid.NewGuid(),
            SourceType = "repository",
            SourceReference = "repo1",
            Status = "completed",
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        var job2 = new IngestionJob
        {
            Id = Guid.NewGuid(),
            SourceType = "blob_container",
            SourceReference = "blob1",
            Status = "pending",
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        var jobsSet = new List<IngestionJob> { job1, job2 }.AsQueryable().BuildMockDbSet();
        _db.IngestionJobs.Returns(jobsSet);

        var query = new GetIngestionJobsQuery(1, 10);
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(job2.Id, result.Items[0].Id);
    }
}
