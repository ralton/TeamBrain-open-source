using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.Threads.Commands.PromoteLearning;

public class PromoteThreadToLearningHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IAiService ai,
    IAuditService audit
) : IRequestHandler<PromoteThreadToLearningCommand, Guid>
{
    public async Task<Guid> Handle(PromoteThreadToLearningCommand request, CancellationToken cancellationToken)
    {
        var thread = await db.Threads
            .Include(t => t.Messages)
            .Include(t => t.SourceArtifacts).ThenInclude(s => s.SourceArtifact)
            .FirstOrDefaultAsync(t => t.Id == request.ThreadId && !t.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException($"Thread {request.ThreadId} not found.");

        var threadSummary = string.Join("\n", thread.Messages.Select(m => $"{m.Role}: {m.Content}"));
        var sourceContents = thread.SourceArtifacts
            .Select(s => s.SourceArtifact.Title)
            .ToList();

        var draft = await ai.DraftLearningRecordAsync(threadSummary, sourceContents, cancellationToken);

        var record = new LearningRecord
        {
            Title = draft.Title,
            Summary = draft.Summary,
            BodyMarkdown = draft.BodyMarkdown,
            Category = draft.SuggestedCategory,
            Domain = draft.SuggestedDomain,
            Status = KnowledgeStatus.Candidate,
            Confidence = ConfidenceLevel.Medium,
            CreatedByUserId = currentUser.UserId
        };

        foreach (var tag in draft.Tags)
            record.Tags.Add(new LearningRecordTag { Tag = tag });

        db.LearningRecords.Add(record);
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("LearningRecord", record.Id, "PromotedFromThread",
            new { ThreadId = thread.Id, draft.Title }, cancellationToken);

        return record.Id;
    }
}
