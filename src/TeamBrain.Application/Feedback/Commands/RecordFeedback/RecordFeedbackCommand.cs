using MediatR;

namespace TeamBrain.Application.Feedback.Commands.RecordFeedback;

public record RecordFeedbackCommand(
    Guid QueryLogId,
    bool IsHelpful,
    string? Comment = null
) : IRequest<Guid>;
