using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamBrain.Application.Common.Interfaces;

namespace TeamBrain.Application.LearningRecords.Queries.SearchKnowledgeRecords;

public class SearchKnowledgeRecordsHandler(
    IApplicationDbContext dbContext,
    ISearchService searchService
) : IRequestHandler<SearchKnowledgeRecordsQuery, SearchKnowledgeRecordsResult>
{
    public async Task<SearchKnowledgeRecordsResult> Handle(SearchKnowledgeRecordsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.LearningRecords
            .AsNoTracking()
            .Include(r => r.Tags)
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(r => r.Category.ToLower() == request.Category.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(request.Domain))
        {
            query = query.Where(r => r.Domain != null && r.Domain.ToLower() == request.Domain.ToLower());
        }

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        if (request.MinimumConfidence.HasValue)
        {
            query = query.Where(r => r.Confidence >= request.MinimumConfidence.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var text = request.SearchText.ToLower();
            query = query.Where(r =>
                r.Title.ToLower().Contains(text) ||
                r.Summary.ToLower().Contains(text) ||
                (r.BodyMarkdown != null && r.BodyMarkdown.ToLower().Contains(text))
            );
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new KnowledgeRecordSearchItem(
                r.Id,
                r.Title,
                r.Summary,
                r.BodyMarkdown,
                r.Category,
                r.Domain,
                r.Status,
                r.Confidence,
                r.Version,
                r.IsAutoPromoted,
                r.Tags.Select(t => t.Tag).ToArray(),
                r.ExternalWorkItemId,
                r.ExternalWorkItemProvider,
                r.ExternalWorkItemUrl,
                r.CreatedAt,
                r.UpdatedAt,
                1.0
            ))
            .ToListAsync(cancellationToken);

        return new SearchKnowledgeRecordsResult(items, totalCount, request.Page, request.PageSize);
    }
}
