using FluentValidation;

namespace TeamBrain.Application.Threads.Commands.UpdateThread;

public class UpdateThreadCommandValidator : AbstractValidator<UpdateThreadCommand>
{
    public UpdateThreadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Thread ID is required.");

        When(x => x.Title != null, () =>
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");
        });
    }
}
