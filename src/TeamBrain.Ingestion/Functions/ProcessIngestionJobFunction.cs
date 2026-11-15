using System.Text.Json;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TeamBrain.Application.Ingestion.Commands.ProcessIngestionJob;

namespace TeamBrain.Ingestion.Functions;

public class ProcessIngestionJobFunction(
    IMediator mediator,
    ILoggerFactory loggerFactory
)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ProcessIngestionJobFunction>();

    [Function(nameof(ProcessIngestionJobFunction))]
    public async Task Run(
        [ServiceBusTrigger("ingestion-jobs", Connection = "ServiceBusConnection")] string messageBody,
        FunctionContext context)
    {
        _logger.LogInformation("Processing ingestion job message: {MessageBody}", messageBody);

        try
        {
            var payload = JsonSerializer.Deserialize<ProcessIngestionJobPayload>(messageBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload is null || payload.JobId == Guid.Empty || string.IsNullOrWhiteSpace(payload.StorageUri))
            {
                _logger.LogWarning("Invalid ingestion job message payload received.");
                return;
            }

            var result = await mediator.Send(new ProcessIngestionJobCommand(
                payload.JobId,
                payload.StorageUri,
                payload.SourceType ?? "Blob"
            ));

            if (result.Success)
            {
                _logger.LogInformation("Successfully processed IngestionJob {JobId}. Processed: {Processed}, Quarantined: {Quarantined}",
                    payload.JobId, result.ArtifactsProcessed, result.ArtifactsQuarantined);
            }
            else
            {
                _logger.LogError("Failed processing IngestionJob {JobId}. Error: {Error}", payload.JobId, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error processing ingestion job message.");
            throw;
        }
    }
}

public record ProcessIngestionJobPayload(
    Guid JobId,
    string StorageUri,
    string? SourceType
);
