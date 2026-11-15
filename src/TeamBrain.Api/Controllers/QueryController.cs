using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamBrain.Application.Query.Commands.ExecuteQuery;

namespace TeamBrain.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class QueryController(IMediator mediator) : ControllerBase
{
    [HttpPost("query")]
    public async Task<IActionResult> Query([FromBody] QueryRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(new ExecuteQueryCommand(
            request.Question,
            request.Context?.RepoId,
            request.Context?.BranchName,
            request.Context?.FilePath,
            request.Context?.Language,
            request.Context?.Framework,
            request.Context?.AdoWorkItemId,
            request.Domain,
            request.IncludeLegacyContext
        ), ct);
        return Ok(result);
    }
}

public record QueryRequest(
    string Question,
    QueryContext? Context,
    string? Domain,
    bool IncludeLegacyContext = false
);

public record QueryContext(
    string? RepoId,
    string? BranchName,
    string? FilePath,
    string? Language,
    string? Framework,
    string? AdoWorkItemId
);
