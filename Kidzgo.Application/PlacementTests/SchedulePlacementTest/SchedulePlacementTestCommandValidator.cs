using FluentValidation;

namespace Kidzgo.Application.PlacementTests.SchedulePlacementTest;

public sealed class SchedulePlacementTestCommandValidator : AbstractValidator<SchedulePlacementTestCommand>
{
    public SchedulePlacementTestCommandValidator()
    {
        RuleFor(command => command.ScheduledAt)
            .NotEmpty()
            .WithMessage("ScheduledAt is required")
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("ScheduledAt cannot be in the past");

        RuleFor(command => command.DurationMinutes)
            .GreaterThan(0)
            .When(command => command.DurationMinutes.HasValue)
            .WithMessage("DurationMinutes must be greater than 0");

        RuleFor(command => command.RoomId)
            .NotEmpty()
            .WithMessage("RoomId is required");

        RuleFor(command => command.InvigilatorUserId)
            .NotEmpty()
            .WithMessage("InvigilatorUserId is required");
    }
}

