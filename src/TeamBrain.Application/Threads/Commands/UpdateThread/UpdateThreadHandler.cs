using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.Threads.Commands.UpdateThread;

public class UpdateThreadHandler(
    IApplicationDbContext dbContext,
    IAuditService auditService
) : IRequestHandler<UpdateThreadCommand, bool>
{
    public async Task<bool> Handle(UpdateThreadCommand request, CancellationToken cancellationToken)
    {
        var thread = await dbContext.Threads
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (thread is null) return false;

        if (request.Title is not null) thread.Title = request.Title;
        if (request.ThreadType.HasValue) thread.ThreadType = request.ThreadType.Value;
        if (request.Status is not null) thread.Status = request.Status;
        if (request.RepoId is not null) thread.RepoId = request.RepoId;
        if (request.BranchName is not null) thread.BranchName = request.BranchName;
        if (request.AdoWorkItemId is not null) thread.AdoWorkItemId = request.AdoWorkItemId;
        if (request.AsanaTaskId is not null) thread.AsanaTaskId = request.AsanaTaskId;
        if (request.Summary is not null) thread.Summary = request.Summary;

        thread.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            "ThreadUpdated",
            "TeamBrainThread",
            thread.Id.ToString(),
            $"Updated Thread '{thread.Title}'",
            cancellationToken
        );

        return true;
    }
}
