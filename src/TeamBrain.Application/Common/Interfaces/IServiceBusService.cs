namespace TeamBrain.Application.Common.Interfaces;

public interface IServiceBusService
{
    Task SendMessageAsync<T>(string queueName, T message, CancellationToken ct = default);
}
