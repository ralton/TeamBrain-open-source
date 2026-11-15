using System.Text.Json;

namespace TeamBrain.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized access");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Forbidden" }));
        }
        catch (Exception ex)
        {
            var correlationId = context.TraceIdentifier;
            logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "An unexpected error occurred.",
                correlationId
            }));
        }
    }
}
