using MediatR;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;

namespace TeamBrain.Application.Threads.Commands.CreateThread;

public class CreateThreadHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IAuditService audit
) : IRequestHandler<CreateThreadCommand, Guid>
{
    public async Task<Guid> Handle(CreateThreadCommand request, CancellationToken cancellationToken)
    {
        var thread = new TeamBrainThread
        {
            Title = request.Title,
            ThreadType = request.ThreadType,
            Status = "active",
            RepoId = request.RepoId,
            BranchName = request.BranchName,
            AdoWorkItemId = request.AdoWorkItemId,
            AsanaTaskId = request.AsanaTaskId,
            CreatedByUserId = currentUser.UserId
        };

        thread.Participants.Add(new ThreadParticipant
        {
            UserId = currentUser.UserId
        });

        db.Threads.Add(thread);
        await db.SaveChangesAsync(cancellationToken);
        await audit.LogAsync("Thread", thread.Id, "Created", new { thread.Title }, cancellationToken);

        return thread.Id;
    }
}
