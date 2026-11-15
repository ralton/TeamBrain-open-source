using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.LearningRecords.Queries.SearchKnowledgeRecords;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;
using Xunit;

namespace TeamBrain.Application.Tests.LearningRecords;

public class SearchKnowledgeRecordsHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ISearchService _searchService;
    private readonly SearchKnowledgeRecordsHandler _handler;

    public SearchKnowledgeRecordsHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _searchService = Substitute.For<ISearchService>();
        _handler = new SearchKnowledgeRecordsHandler(_db, _searchService);
    }

    [Fact]
    public async Task Handle_FacetedFilters_ReturnsMatchingRecords()
    {
        var record1 = new LearningRecord
        {
            Id = Guid.NewGuid(),
            Title = "Approved Retry Pattern",
            Summary = "Retry policy with exponential backoff",
            Category = "Service Pattern",
            Domain = "Core",
            Status = KnowledgeStatus.Approved,
            Confidence = ConfidenceLevel.High,
            IsDeleted = false,
            UpdatedAt = DateTime.UtcNow
        };

        var record2 = new LearningRecord
        {
            Id = Guid.NewGuid(),
            Title = "Legacy Database Helper",
            Summary = "Avoid using static DbContext helpers",
            Category = "Database",
            Domain = "Core",
            Status = KnowledgeStatus.Legacy,
            Confidence = ConfidenceLevel.Medium,
            IsDeleted = false,
            UpdatedAt = DateTime.UtcNow
        };

        var dbSet = new List<LearningRecord> { record1, record2 }.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);

        var query = new SearchKnowledgeRecordsQuery(
            Category: "Service Pattern",
            Status: KnowledgeStatus.Approved
        );

        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(record1.Id, result.Items[0].Id);
        Assert.Equal(KnowledgeStatus.Approved, result.Items[0].Status);
    }

    [Fact]
    public async Task Handle_SearchTextFilter_MatchesTitleAndSummary()
    {
        var record1 = new LearningRecord
        {
            Id = Guid.NewGuid(),
            Title = "Blob Storage Retry Policy",
            Summary = "Handles transient network exceptions",
            Category = "DevOps",
            Status = KnowledgeStatus.Approved,
            IsDeleted = false
        };

        var dbSet = new List<LearningRecord> { record1 }.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);

        var query = new SearchKnowledgeRecordsQuery(SearchText: "Storage");
        var result = await _handler.Handle(query, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(record1.Id, result.Items[0].Id);
    }
}
