namespace TeamBrain.Application.Common.Interfaces;

public interface IAsanaService
{
    Task<AsanaTaskResult?> CreateReviewTaskAsync(AsanaTaskRequest request, CancellationToken cancellationToken = default);
    Task UpdateTaskStatusAsync(string asanaTaskId, string status, CancellationToken cancellationToken = default);
}

public record AsanaTaskRequest(
    string Title,
    string Description,
    string LearningRecordId,
    string? AssigneeEmail
);

public record AsanaTaskResult(string TaskId, string TaskUrl);
