using FluentValidation;

namespace Kidzgo.Application.MakeupCredits.CreateMakeupCredit;

public sealed class CreateMakeupCreditCommandValidator : AbstractValidator<CreateMakeupCreditCommand>
{
    public CreateMakeupCreditCommandValidator()
    {
        RuleFor(command => command.StudentProfileId)
            .NotEmpty()
            .WithMessage("Student Profile ID is required");

        RuleFor(command => command.SourceSessionId)
            .NotEmpty()
            .WithMessage("Source Session ID is required");

        RuleFor(command => command.ExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("ExpiresAt should be in the future")
            .When(command => command.ExpiresAt.HasValue);
    }
}

