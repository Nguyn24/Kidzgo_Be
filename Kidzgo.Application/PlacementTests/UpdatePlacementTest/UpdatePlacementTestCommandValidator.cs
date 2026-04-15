using FluentValidation;

namespace Kidzgo.Application.PlacementTests.UpdatePlacementTest;

public sealed class UpdatePlacementTestCommandValidator : AbstractValidator<UpdatePlacementTestCommand>
{
    public UpdatePlacementTestCommandValidator()
    {
        RuleFor(command => command.PlacementTestId)
            .NotEmpty()
            .WithMessage("Placement Test ID is required");

        RuleFor(command => command.ScheduledAt)
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("ScheduledAt cannot be in the past")
            .When(command => command.ScheduledAt.HasValue);

        RuleFor(command => command.DurationMinutes)
            .GreaterThan(0)
            .When(command => command.DurationMinutes.HasValue)
            .WithMessage("DurationMinutes must be greater than 0");
    }
}

