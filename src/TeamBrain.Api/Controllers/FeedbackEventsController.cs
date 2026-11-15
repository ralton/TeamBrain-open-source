using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamBrain.Application.Feedback.Commands.RecordFeedback;

namespace TeamBrain.Api.Controllers;

[ApiController]
[Route("api/v1/feedback")]
[Authorize]
public class FeedbackEventsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RecordFeedback([FromBody] RecordFeedbackRequest request, CancellationToken ct)
    {
        var id = await mediator.Send(new RecordFeedbackCommand(
            request.QueryLogId,
            request.IsHelpful,
            request.Comment
        ), ct);

        return Ok(new { id });
    }
}

public record RecordFeedbackRequest(
    Guid QueryLogId,
    bool IsHelpful,
    string? Comment
);
