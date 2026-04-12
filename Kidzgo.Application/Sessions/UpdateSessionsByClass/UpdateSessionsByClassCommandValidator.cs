using FluentValidation;

namespace Kidzgo.Application.Sessions.UpdateSessionsByClass;

public sealed class UpdateSessionsByClassCommandValidator : AbstractValidator<UpdateSessionsByClassCommand>
{
    public UpdateSessionsByClassCommandValidator()
    {
        RuleFor(command => command.ClassId)
            .NotEmpty()
            .WithMessage("Class ID is required");

        RuleFor(command => command.PlannedDatetime)
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("PlannedDatetime cannot be in the past")
            .When(command => command.PlannedDatetime.HasValue);

        RuleFor(command => command.Color)
            .MaximumLength(50)
            .WithMessage("Color must not exceed 50 characters")
            .When(command => command.Color is not null);
    }
}

