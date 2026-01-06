using FluentValidation;

namespace Kidzgo.Application.Enrollments.ReactivateEnrollment;

public sealed class ReactivateEnrollmentCommandValidator : AbstractValidator<ReactivateEnrollmentCommand>
{
    public ReactivateEnrollmentCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Enrollment ID is required");
    }
}

