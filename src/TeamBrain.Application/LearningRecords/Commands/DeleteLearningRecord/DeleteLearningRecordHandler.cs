using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.LearningRecords.Commands.DeleteLearningRecord;

public class DeleteLearningRecordHandler(
    IApplicationDbContext db,
    IAuditService audit
) : IRequestHandler<DeleteLearningRecordCommand>
{
    public async Task Handle(DeleteLearningRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await db.LearningRecords
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException($"Learning record {request.Id} not found.");

        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await audit.LogAsync("LearningRecord", record.Id, "SoftDeleted", new { record.Title }, cancellationToken);
    }
}
