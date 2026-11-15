using FluentValidation;

namespace TeamBrain.Application.Ingestion.Commands.SubmitIngestionJob;

public class SubmitIngestionJobCommandValidator : AbstractValidator<SubmitIngestionJobCommand>
{
    public SubmitIngestionJobCommandValidator()
    {
        RuleFor(x => x.SourceType)
            .NotEmpty().WithMessage("SourceType is required.")
            .MaximumLength(50).WithMessage("SourceType cannot exceed 50 characters.");

        RuleFor(x => x.SourceReference)
            .NotEmpty().WithMessage("SourceReference is required.")
            .MaximumLength(1000).WithMessage("SourceReference cannot exceed 1000 characters.");
    }
}
