using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.Ingestion.Commands.ProcessIngestionJob;

public class ProcessIngestionJobHandler(
    IApplicationDbContext db,
    IBlobStorageService blobStorage,
    ISecretScannerService secretScanner,
    IAiService aiService,
    IAuditService audit
) : IRequestHandler<ProcessIngestionJobCommand, ProcessIngestionJobResult>
{
    public async Task<ProcessIngestionJobResult> Handle(ProcessIngestionJobCommand request, CancellationToken cancellationToken)
    {
        var job = await db.IngestionJobs
            .FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);

        if (job is null)
        {
            return new ProcessIngestionJobResult(false, 0, 0, $"IngestionJob {request.JobId} not found.");
        }

        job.Status = "running";
        job.StartedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        try
        {
            using var stream = await blobStorage.DownloadAsync(request.StorageUri, cancellationToken);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var rawContent = await reader.ReadToEndAsync(cancellationToken);

            var scanResult = await secretScanner.ScanAsync(rawContent, cancellationToken);

            var sourceArtifact = new SourceArtifact
            {
                Title = Path.GetFileName(request.StorageUri) ?? "Ingested Artifact",
                ArtifactType = ArtifactType.SourceCode,
                Classification = scanResult.HasSecret ? ArtifactClassification.Quarantined : ArtifactClassification.Approved,
                ContentStorageUri = request.StorageUri,
                IsQuarantined = scanResult.HasSecret
            };

            db.SourceArtifacts.Add(sourceArtifact);

            if (scanResult.HasSecret)
            {
                job.ArtifactsQuarantined++;
                db.IngestionErrors.Add(new IngestionError
                {
                    IngestionJobId = job.Id,
                    ErrorType = "SecretDetected",
                    Message = $"Quarantined content. Found {scanResult.Matches.Count} secret pattern matches.",
                    SourceReference = request.StorageUri
                });

                job.Status = "completed";
                job.CompletedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);

                await audit.LogAsync("IngestionJob", job.Id, "QuarantinedSecrets", new { request.StorageUri }, cancellationToken);
                return new ProcessIngestionJobResult(true, job.ArtifactsProcessed, job.ArtifactsQuarantined, "Secrets detected and quarantined.");
            }

            var redactedContent = secretScanner.Redact(rawContent);
            sourceArtifact.Chunks.Add(new SourceArtifactChunk
            {
                ChunkIndex = 0,
                Content = redactedContent
            });

            var draft = await aiService.DraftLearningRecordAsync(
                threadSummary: $"Ingested from {request.SourceType}: {request.StorageUri}",
                sourceContents: new[] { redactedContent },
                cancellationToken: cancellationToken
            );

            var confidence = ConfidenceLevel.High;
            var isHighConfidence = confidence >= ConfidenceLevel.High;

            var learningRecord = new LearningRecord
            {
                Title = draft.Title,
                Summary = draft.Summary,
                BodyMarkdown = draft.BodyMarkdown,
                Category = string.IsNullOrWhiteSpace(draft.SuggestedCategory) ? "General" : draft.SuggestedCategory,
                Domain = draft.SuggestedDomain,
                Confidence = confidence,
                Status = isHighConfidence ? KnowledgeStatus.InReview : KnowledgeStatus.Candidate,
                IsAutoPromoted = isHighConfidence,
                AutoPromotionReason = isHighConfidence ? "Auto-promoted to InReview via High confidence extraction" : null,
                CreatedByUserId = job.CreatedByUserId
            };

            if (draft.Tags != null)
            {
                foreach (var tag in draft.Tags)
                {
                    learningRecord.Tags.Add(new LearningRecordTag { Tag = tag });
                }
            }

            learningRecord.Sources.Add(new LearningRecordSource
            {
                SourceArtifact = sourceArtifact,
                Classification = ArtifactClassification.Approved
            });

            db.LearningRecords.Add(learningRecord);

            job.ArtifactsProcessed++;
            job.Status = "completed";
            job.CompletedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            await audit.LogAsync("IngestionJob", job.Id, "Completed", new { learningRecord.Id, learningRecord.Title }, cancellationToken);

            return new ProcessIngestionJobResult(true, job.ArtifactsProcessed, job.ArtifactsQuarantined, null);
        }
        catch (Exception ex)
        {
            job.Status = "failed";
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;

            db.IngestionErrors.Add(new IngestionError
            {
                IngestionJobId = job.Id,
                ErrorType = "ProcessingError",
                Message = ex.Message,
                SourceReference = request.StorageUri
            });

            await db.SaveChangesAsync(cancellationToken);
            return new ProcessIngestionJobResult(false, job.ArtifactsProcessed, job.ArtifactsQuarantined, ex.Message);
        }
    }
}
