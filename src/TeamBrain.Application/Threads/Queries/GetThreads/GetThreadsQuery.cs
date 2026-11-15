using MediatR;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.Threads.Queries.GetThreads;

public record GetThreadsQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<GetThreadsResult>;

public record GetThreadsResult(IReadOnlyList<ThreadSummaryDto> Items, int TotalCount);

public record ThreadSummaryDto(
    Guid Id,
    string Title,
    ThreadType ThreadType,
    string Status,
    string? RepoId,
    string? BranchName,
    string? AdoWorkItemId,
    string? AsanaTaskId,
    int MessageCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
