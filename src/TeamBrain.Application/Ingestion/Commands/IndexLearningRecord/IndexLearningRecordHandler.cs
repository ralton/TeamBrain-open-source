using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.Ingestion.Commands.IndexLearningRecord;

public class IndexLearningRecordHandler(
    IApplicationDbContext db,
    ISearchService search
) : IRequestHandler<IndexLearningRecordCommand>
{
    public async Task Handle(IndexLearningRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await db.LearningRecords
            .Include(r => r.Tags)
            .FirstOrDefaultAsync(r => r.Id == request.LearningRecordId && !r.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"Learning record {request.LearningRecordId} not found.");

        if (record.Status != KnowledgeStatus.Approved)
        {
            throw new InvalidOperationException($"Learning record {record.Id} status is '{record.Status}'. Only approved records can be indexed.");
        }

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
