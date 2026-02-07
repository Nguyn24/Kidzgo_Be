using FluentValidation;

namespace Kidzgo.Application.MakeupCredits.ExpireMakeupCredit;

public sealed class ExpireMakeupCreditCommandValidator : AbstractValidator<ExpireMakeupCreditCommand>
{
    public ExpireMakeupCreditCommandValidator()
    {
        RuleFor(command => command.MakeupCreditId)
            .NotEmpty()
            .WithMessage("Makeup Credit ID is required");

        RuleFor(command => command.ExpiresAt)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("ExpiresAt should be in the past or present when expiring")
            .When(command => command.ExpiresAt.HasValue);
    }
}

