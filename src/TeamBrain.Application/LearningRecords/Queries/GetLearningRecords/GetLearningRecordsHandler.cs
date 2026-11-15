using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.LearningRecords.Queries.GetLearningRecords;

public class GetLearningRecordsHandler(IApplicationDbContext db)
    : IRequestHandler<GetLearningRecordsQuery, GetLearningRecordsResult>
{
    public async Task<GetLearningRecordsResult> Handle(GetLearningRecordsQuery request, CancellationToken cancellationToken)
    {
        var query = db.LearningRecords
            .Include(r => r.Tags)
            .Where(r => !r.IsDeleted);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(r => r.Category == request.Category);

        if (!string.IsNullOrWhiteSpace(request.Domain))
            query = query.Where(r => r.Domain == request.Domain);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(r => r.Title.Contains(request.Search) || r.Summary.Contains(request.Search));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new LearningRecordSummary(
                r.Id,
                r.Title,
                r.Summary,
                r.Category,
                r.Domain,
                r.Status,
                r.Confidence,
                r.Tags.Select(t => t.Tag).ToArray(),
                r.ApprovedAt,
                r.ReviewedAt,
                r.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new GetLearningRecordsResult(items, totalCount);
    }
}
