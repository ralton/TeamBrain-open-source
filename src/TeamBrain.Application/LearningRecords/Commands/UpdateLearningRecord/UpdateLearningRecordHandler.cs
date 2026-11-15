using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.LearningRecords.Commands.UpdateLearningRecord;

public class UpdateLearningRecordHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IAuditService audit,
    ISearchService search
) : IRequestHandler<UpdateLearningRecordCommand>
{
    public async Task Handle(UpdateLearningRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await db.LearningRecords
            .Include(r => r.Tags)
            .Include(r => r.Versions)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"Learning record {request.Id} not found.");

        var versionSnapshot = new LearningRecordVersion
        {
            LearningRecordId = record.Id,
            Version = record.Version,
            Title = record.Title,
            Summary = record.Summary,
            BodyMarkdown = record.BodyMarkdown,
            ChangedByUserId = currentUser.UserId
        };
        db.LearningRecordVersions.Add(versionSnapshot);

        if (request.Title is not null) record.Title = request.Title;
        if (request.Summary is not null) record.Summary = request.Summary;
        if (request.BodyMarkdown is not null) record.BodyMarkdown = request.BodyMarkdown;
        if (request.Category is not null) record.Category = request.Category;
        if (request.Domain is not null) record.Domain = request.Domain;
        if (request.Confidence is not null) record.Confidence = request.Confidence.Value;

        if (request.Tags is not null)
        {
            record.Tags.Clear();
            foreach (var tag in request.Tags)
            {
                record.Tags.Add(new LearningRecordTag { Tag = tag });
            }
        }

        record.Version++;
        record.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.LogAsync("LearningRecord", record.Id, "Updated", new { record.Version, record.Title }, cancellationToken);

        if (record.Status == KnowledgeStatus.Approved)
        {
            var doc = new SearchDocument(
                Id: $"lr-{record.Id}",
                RecordType: "learning_record",
                LearningRecordId: record.Id.ToString(),
                ArtifactId: null,
                Title: record.Title,
                Content: record.BodyMarkdown ?? record.Summary,
                Summary: record.Summary,
                Category: record.Category,
                Domain: record.Domain,
                Systems: Array.Empty<string>(),
                Languages: Array.Empty<string>(),
                RepoId: null,
                FilePath: null,
                Status: record.Status,
                Classification: ArtifactClassification.Approved,
                Confidence: record.Confidence,
                Tags: record.Tags.Select(t => t.Tag).ToArray(),
                OwnerTeam: null,
                ApprovedAt: record.ApprovedAt,
                ReviewedAt: record.ReviewedAt,
                ExpiresAt: record.ExpiresAt,
                SourceUrl: $"/learning-records/{record.Id}"
            );
            await search.IndexLearningRecordAsync(doc, cancellationToken);
        }
    }
}
