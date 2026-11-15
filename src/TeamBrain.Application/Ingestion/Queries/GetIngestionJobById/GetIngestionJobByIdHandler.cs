using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.Ingestion.Queries.GetIngestionJobById;

public class GetIngestionJobByIdHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetIngestionJobByIdQuery, IngestionJobDetailDto?>
{
    public async Task<IngestionJobDetailDto?> Handle(GetIngestionJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await dbContext.IngestionJobs
            .AsNoTracking()
            .Include(j => j.Errors)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (job is null) return null;

        return new IngestionJobDetailDto(
            job.Id,
            job.SourceType,
            job.SourceReference,
            job.Status,
            job.ArtifactsProcessed,
            job.ArtifactsQuarantined,
            job.ErrorMessage,
            job.CreatedByUserId,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt,
            job.Errors.Select(e => new IngestionErrorDto(
                e.Id,
                e.ErrorType,
                e.Message,
                e.SourceReference,
                e.CreatedAt
            )).ToList()
        );
    }
}
