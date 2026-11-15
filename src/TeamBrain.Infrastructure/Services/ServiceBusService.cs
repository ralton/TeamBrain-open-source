using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Infrastructure.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceBusService> _logger;

    public ServiceBusService(IConfiguration configuration, ILogger<ServiceBusService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendMessageAsync<T>(string queueName, T message, CancellationToken ct = default)
    {
        var namespaceName = _configuration["ServiceBus:Namespace"];
        var connectionString = _configuration["ServiceBus:ConnectionString"];

        string payload = JsonSerializer.Serialize(message);

        if (!string.IsNullOrEmpty(connectionString))
        {
            await using var client = new ServiceBusClient(connectionString);
            await using var sender = client.CreateSender(queueName);
            await sender.SendMessageAsync(new ServiceBusMessage(payload), ct);
            _logger.LogInformation("Sent message to queue {QueueName} using connection string: {Payload}", queueName, payload);
        }
        else if (!string.IsNullOrEmpty(namespaceName))
        {
            var fqdn = namespaceName.Contains(".servicebus.windows.net") ? namespaceName : $"{namespaceName}.servicebus.windows.net";
            await using var client = new ServiceBusClient(fqdn, new DefaultAzureCredential());
            await using var sender = client.CreateSender(queueName);
            await sender.SendMessageAsync(new ServiceBusMessage(payload), ct);
            _logger.LogInformation("Sent message to queue {QueueName} using Managed Identity: {Payload}", queueName, payload);
        }
        else
        {
            _logger.LogWarning("ServiceBus configuration not present. Mocking message dispatch to queue {QueueName}: {Payload}", queueName, payload);
        }
    }
}
