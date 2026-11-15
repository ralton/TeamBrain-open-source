using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Application.Integrations.Commands.LinkWorkItem;
using TeamBrain.Application.Integrations.Queries.GetGitHubPullRequest;

namespace TeamBrain.Api.Controllers;

[ApiController]
[Route("api/v1/integrations")]
[Authorize]
public class IntegrationsController(
    IMediator mediator,
    IWorkItemService workItemService
) : ControllerBase
{
    [HttpPost("link-workitem")]
    [Authorize(Roles = "Contributor,Reviewer,Admin")]
    public async Task<IActionResult> LinkWorkItem([FromBody] LinkWorkItemRequest request, CancellationToken ct)
    {
        var success = await mediator.Send(new LinkWorkItemCommand(
            request.TargetId,
            request.TargetType,
            request.Provider,
            request.WorkItemId,
            request.WorkItemUrl
        ), ct);

        return success ? Ok(new { success = true }) : NotFound();
    }

    [HttpGet("github/prs/{owner}/{repo}/{prNumber:int}")]
    public async Task<IActionResult> GetGitHubPullRequest(string owner, string repo, int prNumber, CancellationToken ct)
    {
        var result = await mediator.Send(new GetGitHubPullRequestQuery(owner, repo, prNumber), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("workitems/{provider}/{ownerOrProject}/{repoOrBoard}/{itemId}")]
    public async Task<IActionResult> GetWorkItem(string provider, string ownerOrProject, string repoOrBoard, string itemId, CancellationToken ct)
    {
        var result = await workItemService.GetWorkItemAsync(provider, ownerOrProject, repoOrBoard, itemId, ct);
        return result is null ? NotFound() : Ok(result);
    }
}

public record LinkWorkItemRequest(
    Guid TargetId,
    string TargetType,
    string Provider,
    string WorkItemId,
    string WorkItemUrl
);
