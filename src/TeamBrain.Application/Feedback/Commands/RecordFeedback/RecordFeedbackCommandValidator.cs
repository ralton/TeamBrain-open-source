using FluentValidation;

namespace TeamBrain.Application.Feedback.Commands.RecordFeedback;

public class RecordFeedbackCommandValidator : AbstractValidator<RecordFeedbackCommand>
{
    public RecordFeedbackCommandValidator()
    {
        RuleFor(x => x.QueryLogId)
            .NotEmpty().WithMessage("QueryLogId is required.");

        When(x => x.Comment != null, () =>
        {
            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
        });
    }
}
