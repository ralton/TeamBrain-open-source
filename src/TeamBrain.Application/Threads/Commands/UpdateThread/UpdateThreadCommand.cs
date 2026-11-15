using MediatR;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.Threads.Commands.UpdateThread;

public record UpdateThreadCommand(
    Guid Id,
    string? Title = null,
    ThreadType? ThreadType = null,
    string? Status = null,
    string? RepoId = null,
    string? BranchName = null,
    string? AdoWorkItemId = null,
    string? AsanaTaskId = null,
    string? Summary = null
) : IRequest<bool>;
