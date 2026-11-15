using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamBrain.Application.Ingestion.Commands.SubmitIngestionJob;
using TeamBrain.Application.Ingestion.Queries.GetIngestionJobById;
using TeamBrain.Application.Ingestion.Queries.GetIngestionJobs;

namespace TeamBrain.Api.Controllers;

[ApiController]
[Route("api/v1/ingestion/jobs")]
[Authorize(Roles = "Admin,Reviewer")]
public class IngestionJobsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] SubmitIngestionJobRequest request, CancellationToken ct)
    {
        var id = await mediator.Send(new SubmitIngestionJobCommand(request.SourceType, request.SourceReference), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetIngestionJobsQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetIngestionJobByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}

public record SubmitIngestionJobRequest(
    string SourceType,
    string SourceReference
);
