using Microsoft.EntityFrameworkCore;
using NSubstitute;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.LearningRecords.Commands.CreateLearningRecord;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;
using Xunit;

namespace TeamBrain.Application.Tests.LearningRecords;

public class CreateLearningRecordHandlerTests
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;
    private readonly CreateLearningRecordHandler _handler;

    public CreateLearningRecordHandlerTests()
    {
        _db = Substitute.For<IApplicationDbContext>();
        _currentUser = Substitute.For<ICurrentUserService>();
        _audit = Substitute.For<IAuditService>();

        _currentUser.UserId.Returns(Guid.NewGuid());
        _currentUser.IsAuthenticated.Returns(true);

        var records = new List<LearningRecord>();
        var dbSet = records.AsQueryable().BuildMockDbSet();
        _db.LearningRecords.Returns(dbSet);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        _handler = new CreateLearningRecordHandler(_db, _currentUser, _audit);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesRecordAsCandidate()
    {
        var cmd = new CreateLearningRecordCommand(
            "Test Pattern",
            "A test summary",
            null,
            "Code Pattern",
            "Testing",
            ConfidenceLevel.High,
            new[] { "test", "pattern" },
            null, null, null
        );

        await _handler.Handle(cmd, CancellationToken.None);

        _db.LearningRecords.Received(1).Add(Arg.Is<LearningRecord>(r =>
            r.Title == "Test Pattern" &&
            r.Status == KnowledgeStatus.Candidate
        ));
    }
}
