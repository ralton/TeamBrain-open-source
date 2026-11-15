using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;

namespace TeamBrain.Application.Threads.Commands.AddMessage;

public class AddThreadMessageHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser
) : IRequestHandler<AddThreadMessageCommand, Guid>
{
    public async Task<Guid> Handle(AddThreadMessageCommand request, CancellationToken cancellationToken)
    {
        var thread = await db.Threads
            .FirstOrDefaultAsync(t => t.Id == request.ThreadId && !t.IsDeleted, cancellationToken)
            ?? throw new InvalidOperationException($"Thread {request.ThreadId} not found.");

        var message = new ThreadMessage
        {
            ThreadId = thread.Id,
            AuthorUserId = currentUser.UserId,
            Role = "user",
            Content = request.Content
        };

        db.ThreadMessages.Add(message);
        thread.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return message.Id;
    }
}
