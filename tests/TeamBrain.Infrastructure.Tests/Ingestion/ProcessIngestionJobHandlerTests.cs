using System.Text;
using MockQueryable.NSubstitute;
using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.Ingestion.Commands.ProcessIngestionJob;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;
using Xunit;

namespace TeamBrain.Infrastructure.Tests.Ingestion;

public class ProcessIngestionJobHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly IBlobStorageService _blobStorage;
    private readonly ISecretScannerService _secretScanner;
    private readonly IAiService _aiService;
    private readonly IAuditService _audit;
    private readonly ProcessIngestionJobHandler _handler;

    public ProcessIngestionJobHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _blobStorage = Substitute.For<IBlobStorageService>();
        _secretScanner = Substitute.For<ISecretScannerService>();
        _aiService = Substitute.For<IAiService>();
        _audit = Substitute.For<IAuditService>();

        var jobs = new List<IngestionJob>();
        _db.IngestionJobs.Returns(jobs.AsQueryable().BuildMockDbSet());
        _db.SourceArtifacts.Returns(new List<SourceArtifact>().AsQueryable().BuildMockDbSet());
        _db.LearningRecords.Returns(new List<LearningRecord>().AsQueryable().BuildMockDbSet());
        _db.IngestionErrors.Returns(new List<IngestionError>().AsQueryable().BuildMockDbSet());

        _handler = new ProcessIngestionJobHandler(_db, _blobStorage, _secretScanner, _aiService, _audit);
    }

    [Fact]
    public async Task Handle_JobNotFound_ReturnsFailure()
    {
        var cmd = new ProcessIngestionJobCommand(Guid.NewGuid(), "https://storage.blob.core.windows.net/container/file.cs", "Blob");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task Handle_SecretDetected_QuarantinesArtifact()
    {
        var jobId = Guid.NewGuid();
        var job = new IngestionJob
        {
            Id = jobId,
            Status = "pending"
        };

        _db.IngestionJobs.Returns(new List<IngestionJob> { job }.AsQueryable().BuildMockDbSet());

        var fakeKey = new string('A', 86) + "==";
        var content = $"var secret = \"DefaultEndpointsProtocol=https;AccountName=test;AccountKey={fakeKey}\";";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        _blobStorage.DownloadAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(stream);

        _secretScanner.ScanAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new SecretScanResult(true, new[] { new SecretMatch("AzureStorageKey", 0, "[REDACTED]") }));

        var cmd = new ProcessIngestionJobCommand(jobId, "https://storage/blob.cs", "Blob");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.ArtifactsQuarantined);
        Assert.Equal("completed", job.Status);
        _db.SourceArtifacts.Received(1).Add(Arg.Is<SourceArtifact>(sa => sa.IsQuarantined == true && sa.Classification == ArtifactClassification.Quarantined));
        _db.IngestionErrors.Received(1).Add(Arg.Is<IngestionError>(ie => ie.ErrorType == "SecretDetected"));
    }

    [Fact]
    public async Task Handle_CleanContent_CreatesLearningRecordAndProcessesArtifact()
    {
        var jobId = Guid.NewGuid();
        var job = new IngestionJob
        {
            Id = jobId,
            Status = "pending",
            CreatedByUserId = Guid.NewGuid()
        };

        _db.IngestionJobs.Returns(new List<IngestionJob> { job }.AsQueryable().BuildMockDbSet());

        var content = "public class ApprovedPattern { public void Execute() {} }";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        _blobStorage.DownloadAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(stream);

        _secretScanner.ScanAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new SecretScanResult(false, Array.Empty<SecretMatch>()));
        _secretScanner.Redact(Arg.Any<string>()).Returns(content);

        _aiService.DraftLearningRecordAsync(Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(new AiDraftResponse("Clean Pattern", "A clean architecture pattern", "# Markdown", new[] { "clean" }, "Architecture", "Core"));

        var cmd = new ProcessIngestionJobCommand(jobId, "https://storage/CleanPattern.cs", "Blob");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(1, result.ArtifactsProcessed);
        Assert.Equal(0, result.ArtifactsQuarantined);
        Assert.Equal("completed", job.Status);

        _db.LearningRecords.Received(1).Add(Arg.Is<LearningRecord>(lr =>
            lr.Title == "Clean Pattern" &&
            lr.Status == KnowledgeStatus.InReview &&
            lr.IsAutoPromoted == true
        ));
    }
}
