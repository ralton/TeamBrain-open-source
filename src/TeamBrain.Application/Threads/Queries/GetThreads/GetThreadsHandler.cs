using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.Threads.Queries.GetThreads;

public class GetThreadsHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser
) : IRequestHandler<GetThreadsQuery, GetThreadsResult>
{
    public async Task<GetThreadsResult> Handle(GetThreadsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Threads
            .AsNoTracking()
            .Where(t => !t.IsDeleted && (t.CreatedByUserId == currentUser.UserId || t.Participants.Any(p => p.UserId == currentUser.UserId)));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new ThreadSummaryDto(
                t.Id,
                t.Title,
                t.ThreadType,
                t.Status,
                t.RepoId,
                t.BranchName,
                t.AdoWorkItemId,
                t.AsanaTaskId,
                t.Messages.Count,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new GetThreadsResult(items, totalCount);
    }
}
