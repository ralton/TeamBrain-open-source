using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.Ingestion.Queries.GetIngestionJobs;

public class GetIngestionJobsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetIngestionJobsQuery, IngestionJobsListVm>
{
    public async Task<IngestionJobsListVm> Handle(GetIngestionJobsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.IngestionJobs.AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(j => new IngestionJobDto(
                j.Id,
                j.SourceType,
                j.SourceReference,
                j.Status,
                j.ArtifactsProcessed,
                j.ArtifactsQuarantined,
                j.ErrorMessage,
                j.CreatedByUserId,
                j.CreatedAt,
                j.StartedAt,
                j.CompletedAt
            ))
            .ToListAsync(cancellationToken);

        return new IngestionJobsListVm(items, totalCount, request.Page, request.PageSize);
    }
}
