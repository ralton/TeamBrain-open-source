using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamBrain.Application.Analytics.Queries.GetPilotMetrics;

namespace TeamBrain.Api.Controllers;

[ApiController]
[Route("api/v1/analytics")]
[Authorize]
public class AnalyticsController(IMediator mediator) : ControllerBase
{
    [HttpGet("pilot-metrics")]
    public async Task<IActionResult> GetPilotMetrics(CancellationToken ct)
    {
        var result = await mediator.Send(new GetPilotMetricsQuery(), ct);
        return Ok(result);
    }
}
