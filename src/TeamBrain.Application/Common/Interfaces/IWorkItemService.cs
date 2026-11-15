namespace TeamBrain.Application.Common.Interfaces;

public interface IWorkItemService
{
    Task<WorkItemDetailsResult?> GetWorkItemAsync(string provider, string ownerOrProject, string repoOrBoard, string itemId, CancellationToken ct = default);
    Task<WorkItemTaskResult?> CreateReviewTaskAsync(string provider, CreateWorkItemTaskRequest request, CancellationToken ct = default);
}

public record WorkItemDetailsResult(
    string Id,
    string Provider, // github | ado | asana
    string Title,
    string Status,
    string? Assignee,
    string HtmlUrl,
    DateTime CreatedAt
);

public record CreateWorkItemTaskRequest(
    string OwnerOrProject,
    string RepoOrBoard,
    string Title,
    string BodyDescription,
    string LearningRecordId,
    string? Assignee
);

public record WorkItemTaskResult(
    string Id,
    string HtmlUrl,
    string Provider
);
