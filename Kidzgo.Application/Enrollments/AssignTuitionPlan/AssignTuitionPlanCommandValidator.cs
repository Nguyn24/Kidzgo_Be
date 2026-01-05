using FluentValidation;

namespace Kidzgo.Application.Enrollments.AssignTuitionPlan;

public sealed class AssignTuitionPlanCommandValidator : AbstractValidator<AssignTuitionPlanCommand>
{
    public AssignTuitionPlanCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Enrollment ID is required");

        RuleFor(command => command.TuitionPlanId)
            .NotEmpty().WithMessage("Tuition Plan ID is required");
    }
}

