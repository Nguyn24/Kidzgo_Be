using FluentValidation;

namespace Kidzgo.Application.PlacementTests.SchedulePlacementTest;

public sealed class SchedulePlacementTestCommandValidator : AbstractValidator<SchedulePlacementTestCommand>
{
    public SchedulePlacementTestCommandValidator()
    {
        RuleFor(command => command.ScheduledAt)
            .NotEmpty()
            .WithMessage("ScheduledAt is required")
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("ScheduledAt cannot be in the past");
    }
}

