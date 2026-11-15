using MediatR;
using TeamBrain.Application.Common.Interfaces;
using TeamBrain.Domain.Entities;

namespace TeamBrain.Application.Feedback.Commands.RecordFeedback;

public class RecordFeedbackHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IAuditService auditService
) : IRequestHandler<RecordFeedbackCommand, Guid>
{
    public async Task<Guid> Handle(RecordFeedbackCommand request, CancellationToken cancellationToken)
    {
        var feedback = new FeedbackEvent
        {
            Id = Guid.NewGuid(),
            QueryLogId = request.QueryLogId,
            UserId = currentUserService.IsAuthenticated ? currentUserService.UserId : null,
            IsHelpful = request.IsHelpful,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.FeedbackEvents.Add(feedback);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditService.LogAsync(
            "FeedbackSubmitted",
            "FeedbackEvent",
            feedback.Id.ToString(),
            $"User submitted {(request.IsHelpful ? "Helpful" : "Not Helpful")} feedback for QueryLog #{request.QueryLogId}",
            cancellationToken
        );

        return feedback.Id;
    }
}
