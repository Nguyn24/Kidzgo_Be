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
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("ScheduledAt cannot be in the past")
            .When(command => command.ScheduledAt.HasValue);
    }
}

