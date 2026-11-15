using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamBrain.Application.Threads.Commands.AddMessage;
using TeamBrain.Application.Threads.Commands.CreateThread;
using TeamBrain.Application.Threads.Commands.PromoteLearning;
using TeamBrain.Application.Threads.Queries.GetThread;
using TeamBrain.Application.Threads.Queries.GetThreads;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Api.Controllers;

[ApiController]
[Route("api/v1/threads")]
[Authorize]
public class ThreadsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetThreadsQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateThreadRequest request, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateThreadCommand(
            request.Title,
            request.ThreadType,
            request.RepoId,
            request.BranchName,
            request.AdoWorkItemId,
            request.AsanaTaskId
        ), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetThreadQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/messages")]
    [Authorize(Roles = "Contributor,Reviewer,Admin")]
    public async Task<IActionResult> AddMessage(Guid id, [FromBody] AddMessageRequest request, CancellationToken ct)
    {
        var messageId = await mediator.Send(new AddThreadMessageCommand(id, request.Content), ct);
        return Ok(new { messageId });
    }

    [HttpPost("{id:guid}/promote-learning")]
    [Authorize(Roles = "Contributor,Reviewer,Admin")]
    public async Task<IActionResult> PromoteLearning(Guid id, CancellationToken ct)
    {
        var learningRecordId = await mediator.Send(new PromoteThreadToLearningCommand(id), ct);
        return Ok(new { learningRecordId });
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Contributor,Reviewer,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateThreadRequest request, CancellationToken ct)
    {
        var success = await mediator.Send(new TeamBrain.Application.Threads.Commands.UpdateThread.UpdateThreadCommand(
            id,
            request.Title,
            request.ThreadType,
            request.Status,
            request.RepoId,
            request.BranchName,
            request.AdoWorkItemId,
            request.AsanaTaskId,
            request.Summary
        ), ct);
        return success ? NoContent() : NotFound();
    }
}

public record CreateThreadRequest(
    string Title,
    ThreadType ThreadType,
    string? RepoId,
    string? BranchName,
    string? AdoWorkItemId,
    string? AsanaTaskId
);

public record UpdateThreadRequest(
    string? Title,
    ThreadType? ThreadType,
    string? Status,
    string? RepoId,
    string? BranchName,
    string? AdoWorkItemId,
    string? AsanaTaskId,
    string? Summary
);

public record AddMessageRequest(string Content);

