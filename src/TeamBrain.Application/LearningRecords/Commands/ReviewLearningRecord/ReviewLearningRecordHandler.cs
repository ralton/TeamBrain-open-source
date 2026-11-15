using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.LearningRecords.Commands.ReviewLearningRecord;

public class ReviewLearningRecordHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IAuditService audit,
    IAsanaService? asana,
    ISearchService search
) : IRequestHandler<ReviewLearningRecordCommand>
{
    public async Task Handle(ReviewLearningRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await db.LearningRecords
            .Include(r => r.Tags)
            .Include(r => r.Reviews)
            .FirstOrDefaultAsync(r => r.Id == request.LearningRecordId && !r.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException($"Learning record {request.LearningRecordId} not found.");

        var previousStatus = record.Status;
        var isSme = currentUser.IsInRole("Reviewer") || currentUser.IsInRole("Admin");

        ApplyDecision(record, request, isSme, currentUser.UserId);
        ApplyEdits(record, request);

        record.UpdatedAt = DateTime.UtcNow;
        record.ReviewedAt = DateTime.UtcNow;

        var review = new Review
        {
            LearningRecordId = record.Id,
            ReviewerUserId = currentUser.UserId,
            Decision = request.Decision,
            Comments = request.Comments
        };
        db.Reviews.Add(review);

        await db.SaveChangesAsync(cancellationToken);

        await audit.LogAsync("LearningRecord", record.Id, $"Review:{request.Decision}",
            new { previousStatus, newStatus = record.Status, request.Comments, isSme }, cancellationToken);

        if (record.Status == KnowledgeStatus.Approved)
            await IndexApprovedRecord(record, cancellationToken);

        if (asana is not null)
            await TrySyncAsana(record, request.Decision, cancellationToken);
    }

    private static void ApplyDecision(Domain.Entities.LearningRecord record, ReviewLearningRecordCommand request, bool isSme, Guid currentUserId)
    {
        if (request.Decision == ReviewDecision.Approve)
        {
            var existingPeerApprovals = record.Reviews.Count(r => r.Decision == ReviewDecision.Approve);
            var totalPeerApprovals = existingPeerApprovals + 1;

            if (isSme || totalPeerApprovals >= record.RequiredPeerApprovals)
            {
                record.Status = KnowledgeStatus.Approved;
                record.ApprovedAt = DateTime.UtcNow;
                record.ApprovedByUserId = currentUserId;
                if (!isSme)
                {
                    record.AutoPromotionReason = $"Auto-approved via {totalPeerApprovals} peer review approvals.";
                }
            }
            else
            {
                record.Status = KnowledgeStatus.InReview;
            }
        }
        else
        {
            record.Status = request.Decision switch
            {
                ReviewDecision.Reject => KnowledgeStatus.Rejected,
                ReviewDecision.MarkLegacy => KnowledgeStatus.Legacy,
                ReviewDecision.MarkDeprecated => KnowledgeStatus.Deprecated,
                ReviewDecision.Archive => KnowledgeStatus.Archived,
                _ => KnowledgeStatus.InReview
            };
        }
    }

    private static void ApplyEdits(Domain.Entities.LearningRecord record, ReviewLearningRecordCommand request)
    {
        if (request.UpdatedTitle is not null) record.Title = request.UpdatedTitle;
        if (request.UpdatedSummary is not null) record.Summary = request.UpdatedSummary;
        if (request.UpdatedBodyMarkdown is not null) record.BodyMarkdown = request.UpdatedBodyMarkdown;
        if (request.UpdatedConfidence is not null) record.Confidence = request.UpdatedConfidence.Value;
        if (request.ExpiresAt is not null) record.ExpiresAt = request.ExpiresAt;
    }

    private async Task IndexApprovedRecord(Domain.Entities.LearningRecord record, CancellationToken cancellationToken)
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

    private async Task TrySyncAsana(Domain.Entities.LearningRecord record, ReviewDecision decision, CancellationToken cancellationToken)
    {
        try
        {
            var asanaLink = await db.FeedbackEvents // placeholder — real implementation looks up AsanaLinks table
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            // Asana sync is best-effort; failure must not fail the review
        }
        catch { /* swallow — integration failure must not block review */ }
    }
}
