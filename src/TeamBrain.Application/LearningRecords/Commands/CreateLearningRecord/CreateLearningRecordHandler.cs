using MediatR;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.LearningRecords.Commands.CreateLearningRecord;

public class CreateLearningRecordHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IAuditService audit
) : IRequestHandler<CreateLearningRecordCommand, Guid>
{
    public async Task<Guid> Handle(CreateLearningRecordCommand request, CancellationToken cancellationToken)
    {
        var isHighConfidence = request.Confidence >= ConfidenceLevel.High;

        var record = new LearningRecord
        {
            Title = request.Title,
            Summary = request.Summary,
            BodyMarkdown = request.BodyMarkdown,
            Category = request.Category,
            Domain = request.Domain,
            Confidence = request.Confidence,
            Status = isHighConfidence ? KnowledgeStatus.InReview : KnowledgeStatus.Candidate,
            IsAutoPromoted = isHighConfidence,
            AutoPromotionReason = isHighConfidence ? "Auto-promoted to InReview due to High confidence level" : null,
            CreatedByUserId = currentUser.UserId,
            OwnerTeamId = request.OwnerTeamId,
            OwnerUserId = request.OwnerUserId,
        };

        foreach (var tag in request.Tags)
            record.Tags.Add(new LearningRecordTag { Tag = tag });

        db.LearningRecords.Add(record);
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("LearningRecord", record.Id, "Created", new { record.Title, record.Status }, cancellationToken);

        return record.Id;
    }
}
