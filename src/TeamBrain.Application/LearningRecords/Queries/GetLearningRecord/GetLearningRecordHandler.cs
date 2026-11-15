using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.LearningRecords.Queries.GetLearningRecord;

public class GetLearningRecordHandler(IApplicationDbContext db)
    : IRequestHandler<GetLearningRecordQuery, LearningRecordDetail?>
{
    public async Task<LearningRecordDetail?> Handle(GetLearningRecordQuery request, CancellationToken cancellationToken)
    {
        var record = await db.LearningRecords
            .Include(r => r.Tags)
            .Include(r => r.Sources).ThenInclude(s => s.SourceArtifact)
            .Include(r => r.Reviews)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (record is null) return null;

        return new LearningRecordDetail(
            record.Id,
            record.Title,
            record.Summary,
            record.BodyMarkdown,
            record.Category,
            record.Domain,
            record.Status,
            record.Confidence,
            record.Version,
            record.Tags.Select(t => t.Tag).ToArray(),
            record.OwnerTeamId,
            record.OwnerUserId,
            record.CreatedByUserId,
            record.ApprovedByUserId,
            record.ReviewedAt,
            record.ApprovedAt,
            record.ExpiresAt,
            record.CreatedAt,
            record.UpdatedAt,
            record.Sources.Select(s => new SourceSummary(
                s.SourceArtifact.Id,
                s.SourceArtifact.Title,
                s.SourceArtifact.ArtifactType.ToString(),
                s.Classification.ToString(),
                s.SourceArtifact.SourceUrl
            )).ToList(),
            record.Reviews.Select(r => new ReviewSummary(
                r.ReviewerUserId,
                r.Decision.ToString(),
                r.Comments,
                r.CreatedAt
            )).ToList()
        );
    }
}
