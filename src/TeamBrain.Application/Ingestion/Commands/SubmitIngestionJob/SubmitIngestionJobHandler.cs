using MediatR;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;

namespace TeamBrain.Application.Ingestion.Commands.SubmitIngestionJob;

public class SubmitIngestionJobHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IServiceBusService serviceBusService
) : IRequestHandler<SubmitIngestionJobCommand, Guid>
{
    public async Task<Guid> Handle(SubmitIngestionJobCommand request, CancellationToken cancellationToken)
    {
        var job = new IngestionJob
        {
            Id = Guid.NewGuid(),
            SourceType = request.SourceType,
            SourceReference = request.SourceReference,
            Status = "pending",
            CreatedByUserId = currentUserService.UserId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.IngestionJobs.Add(job);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Send job payload to Azure Service Bus ingestion-jobs queue for Azure Functions processing
        await serviceBusService.SendMessageAsync("ingestion-jobs", new
        {
            jobId = job.Id,
            storageUri = job.SourceReference,
            sourceType = job.SourceType
        }, cancellationToken);

        return job.Id;
    }
}
