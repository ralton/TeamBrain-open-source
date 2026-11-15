namespace TeamBrain.Application.Common.Interfaces;

public interface IAuditService
{
    Task LogAsync(string entityType, Guid entityId, string action, object? payload = null, CancellationToken cancellationToken = default);
}
