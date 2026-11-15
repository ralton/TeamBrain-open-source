using FluentValidation;

namespace TeamBrain.Application.LearningRecords.Commands.UpdateLearningRecord;

public class UpdateLearningRecordCommandValidator : AbstractValidator<UpdateLearningRecordCommand>
{
    public UpdateLearningRecordCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
            .When(x => x.Title != null);
        RuleFor(x => x.Summary)
            .MaximumLength(2000).WithMessage("Summary cannot exceed 2000 characters.")
            .When(x => x.Summary != null);
    }
}
