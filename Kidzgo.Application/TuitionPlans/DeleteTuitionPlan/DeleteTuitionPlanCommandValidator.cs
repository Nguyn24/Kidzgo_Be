using FluentValidation;

namespace Kidzgo.Application.TuitionPlans.DeleteTuitionPlan;

public sealed class DeleteTuitionPlanCommandValidator : AbstractValidator<DeleteTuitionPlanCommand>
{
    public DeleteTuitionPlanCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Tuition Plan ID is required");
    }
}

