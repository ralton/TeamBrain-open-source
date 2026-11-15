using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.Threads.Queries.GetThread;

public class GetThreadHandler(IApplicationDbContext db)
    : IRequestHandler<GetThreadQuery, ThreadDetail?>
{
    public async Task<ThreadDetail?> Handle(GetThreadQuery request, CancellationToken cancellationToken)
    {
        var thread = await db.Threads
            .Include(t => t.Messages)
            .Include(t => t.Participants)
            .Include(t => t.SourceArtifacts).ThenInclude(s => s.SourceArtifact)
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (thread is null) return null;

        return new ThreadDetail(
            thread.Id,
            thread.Title,
            thread.ThreadType.ToString(),
            thread.Status,
            thread.RepoId,
            thread.BranchName,
            thread.AdoWorkItemId,
            thread.AsanaTaskId,
            thread.Summary,
            thread.CreatedByUserId,
            thread.CreatedAt,
            thread.UpdatedAt,
            thread.Messages.OrderBy(m => m.CreatedAt)
                .Select(m => new ThreadMessageDto(m.Id, m.AuthorUserId, m.Role, m.Content, m.CreatedAt))
                .ToList(),
            thread.Participants.Select(p => p.UserId).ToList(),
            thread.SourceArtifacts.Select(s => new ThreadArtifactDto(
                s.SourceArtifact.Id,
                s.SourceArtifact.Title,
                s.SourceArtifact.ArtifactType.ToString(),
                s.SourceArtifact.Classification.ToString(),
                s.SourceArtifact.SourceUrl
            )).ToList()
        );
    }
}
