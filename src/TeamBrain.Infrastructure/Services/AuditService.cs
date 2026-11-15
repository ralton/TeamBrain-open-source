using System.Text.Json;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;

namespace TeamBrain.Infrastructure.Services;

public class AuditService(IApplicationDbContext db, ICurrentUserService currentUser) : IAuditService
{
    public async Task LogAsync(string entityType, Guid entityId, string action, object? payload = null, CancellationToken cancellationToken = default)
    {
        var evt = new AuditEvent
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = currentUser.IsAuthenticated ? currentUser.UserId : null,
            PayloadJson = payload is not null ? JsonSerializer.Serialize(payload) : null
        };
        db.AuditEvents.Add(evt);
        await db.SaveChangesAsync(cancellationToken);
    }
}
