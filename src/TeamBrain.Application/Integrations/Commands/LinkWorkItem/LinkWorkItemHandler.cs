using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.Integrations.Commands.LinkWorkItem;

public class LinkWorkItemHandler(
    IApplicationDbContext dbContext,
    IAuditService auditService
) : IRequestHandler<LinkWorkItemCommand, bool>
{
    public async Task<bool> Handle(LinkWorkItemCommand request, CancellationToken cancellationToken)
    {
        if (request.TargetType.Equals("thread", StringComparison.OrdinalIgnoreCase))
        {
            var thread = await dbContext.Threads
                .FirstOrDefaultAsync(t => t.Id == request.TargetId && !t.IsDeleted, cancellationToken);

            if (thread is null) return false;

            thread.ExternalWorkItemId = request.WorkItemId;
            thread.ExternalWorkItemProvider = request.Provider.ToLowerInvariant();
            thread.ExternalWorkItemUrl = request.WorkItemUrl;
            thread.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            await auditService.LogAsync(
                "WorkItemLinked",
                "TeamBrainThread",
                thread.Id.ToString(),
                $"Linked {request.Provider} work item #{request.WorkItemId} to Thread",
                cancellationToken
            );

            return true;
        }
        else if (request.TargetType.Equals("learning_record", StringComparison.OrdinalIgnoreCase))
        {
            var record = await dbContext.LearningRecords
                .FirstOrDefaultAsync(r => r.Id == request.TargetId && !r.IsDeleted, cancellationToken);

            if (record is null) return false;

            record.ExternalWorkItemId = request.WorkItemId;
            record.ExternalWorkItemProvider = request.Provider.ToLowerInvariant();
            record.ExternalWorkItemUrl = request.WorkItemUrl;
            record.UpdatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            await auditService.LogAsync(
                "WorkItemLinked",
                "LearningRecord",
                record.Id.ToString(),
                $"Linked {request.Provider} work item #{request.WorkItemId} to Learning Record",
                cancellationToken
            );

            return true;
        }

        return false;
    }
}
