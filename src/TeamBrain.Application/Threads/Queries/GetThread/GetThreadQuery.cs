using MediatR;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Application.Threads.Queries.GetThread;

public record GetThreadQuery(Guid Id) : IRequest<ThreadDetail?>;

public record ThreadDetail(
    Guid Id,
    string Title,
    string ThreadType,
    string Status,
    string? RepoId,
    string? BranchName,
    string? AdoWorkItemId,
    string? AsanaTaskId,
    string? Summary,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ThreadMessageDto> Messages,
    IReadOnlyList<Guid> ParticipantIds,
    IReadOnlyList<ThreadArtifactDto> SourceArtifacts
);

public record ThreadMessageDto(Guid Id, Guid AuthorUserId, string Role, string Content, DateTime CreatedAt);
public record ThreadArtifactDto(Guid Id, string Title, string ArtifactType, string Classification, string? SourceUrl);
