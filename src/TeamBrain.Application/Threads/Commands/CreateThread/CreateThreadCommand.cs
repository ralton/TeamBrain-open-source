using MediatR;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.Threads.Commands.CreateThread;

public record CreateThreadCommand(
    string Title,
    ThreadType ThreadType,
    string? RepoId,
    string? BranchName,
    string? AdoWorkItemId,
    string? AsanaTaskId
) : IRequest<Guid>;
