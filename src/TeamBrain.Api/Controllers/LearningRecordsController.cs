using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamBrain.Application.LearningRecords.Commands.CreateLearningRecord;
using TeamBrain.Application.LearningRecords.Commands.DeleteLearningRecord;
using TeamBrain.Application.LearningRecords.Commands.ReviewLearningRecord;
using TeamBrain.Application.LearningRecords.Commands.UpdateLearningRecord;
using TeamBrain.Application.LearningRecords.Queries.GetLearningRecord;
using TeamBrain.Application.LearningRecords.Queries.GetLearningRecords;
using TeamBrain.Domain.Enums;

namespace TeamBrain.Api.Controllers;

[ApiController]
[Route("api/v1/learning-records")]
[Authorize]
public class LearningRecordsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] KnowledgeStatus? status,
        [FromQuery] string? category,
        [FromQuery] string? domain,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetLearningRecordsQuery(status, category, domain, search, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? searchText,
        [FromQuery] string? category,
        [FromQuery] string? domain,
        [FromQuery] KnowledgeStatus? status,
        [FromQuery] ConfidenceLevel? minimumConfidence,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new TeamBrain.Application.LearningRecords.Queries.SearchKnowledgeRecords.SearchKnowledgeRecordsQuery(
            searchText,
            category,
            domain,
            status,
            minimumConfidence,
            page,
            pageSize
        ), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLearningRecordQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Contributor,Reviewer,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateLearningRecordRequest request, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateLearningRecordCommand(
            request.Title,
            request.Summary,
            request.BodyMarkdown,
            request.Category,
            request.Domain,
            request.Confidence,
            request.Tags ?? Array.Empty<string>(),
            request.OwnerTeamId,
            request.OwnerUserId,
            request.ThreadId
        ), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Contributor,Reviewer,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLearningRecordRequest request, CancellationToken ct)
    {
        await mediator.Send(new UpdateLearningRecordCommand(
            id,
            request.Title,
            request.Summary,
            request.BodyMarkdown,
            request.Category,
            request.Domain,
            request.Confidence,
            request.Tags
        ), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/review")]
    [Authorize(Roles = "Contributor,Reviewer,Admin")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewRequest request, CancellationToken ct)
    {
        await mediator.Send(new ReviewLearningRecordCommand(
            id,
            request.Decision,
            request.Comments,
            request.UpdatedTitle,
            request.UpdatedSummary,
            request.UpdatedBodyMarkdown,
            request.UpdatedConfidence,
            request.ExpiresAt
        ), ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteLearningRecordCommand(id), ct);
        return NoContent();
    }
}

public record CreateLearningRecordRequest(
    string Title,
    string Summary,
    string? BodyMarkdown,
    string Category,
    string? Domain,
    ConfidenceLevel Confidence,
    string[]? Tags,
    Guid? OwnerTeamId,
    Guid? OwnerUserId,
    Guid? ThreadId
);

public record UpdateLearningRecordRequest(
    string? Title,
    string? Summary,
    string? BodyMarkdown,
    string? Category,
    string? Domain,
    ConfidenceLevel? Confidence,
    string[]? Tags
);

public record ReviewRequest(
    ReviewDecision Decision,
    string? Comments,
    string? UpdatedTitle,
    string? UpdatedSummary,
    string? UpdatedBodyMarkdown,
    ConfidenceLevel? UpdatedConfidence,
    DateTime? ExpiresAt
);

