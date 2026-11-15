using System.Text.Json;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TeamBrain.Application.Ingestion.Commands.IndexLearningRecord;

namespace TeamBrain.Ingestion.Functions;

public class IndexLearningRecordFunction(
    IMediator mediator,
    ILoggerFactory loggerFactory
)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<IndexLearningRecordFunction>();

    [Function(nameof(IndexLearningRecordFunction))]
    public async Task Run(
        [ServiceBusTrigger("indexing-jobs", Connection = "ServiceBusConnection")] string messageBody,
        FunctionContext context)
    {
        _logger.LogInformation("Processing indexing job message: {MessageBody}", messageBody);

        try
        {
            var payload = JsonSerializer.Deserialize<IndexLearningRecordPayload>(messageBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload is null || payload.LearningRecordId == Guid.Empty)
            {
                _logger.LogWarning("Invalid indexing job message payload received.");
                return;
            }

            await mediator.Send(new IndexLearningRecordCommand(payload.LearningRecordId));

            _logger.LogInformation("Successfully indexed LearningRecord {LearningRecordId}", payload.LearningRecordId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error indexing learning record message.");
            throw;
        }
    }
}

public record IndexLearningRecordPayload(
    Guid LearningRecordId
);
