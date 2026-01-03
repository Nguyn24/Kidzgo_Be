using FluentValidation;

namespace Kidzgo.Application.TuitionPlans.ToggleTuitionPlanStatus;

public sealed class ToggleTuitionPlanStatusCommandValidator : AbstractValidator<ToggleTuitionPlanStatusCommand>
{
    public ToggleTuitionPlanStatusCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Tuition Plan ID is required");
    }
}

