using FluentValidation;

namespace Kidzgo.Application.Enrollments.PauseEnrollment;

public sealed class PauseEnrollmentCommandValidator : AbstractValidator<PauseEnrollmentCommand>
{
    public PauseEnrollmentCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Enrollment ID is required");
    }
}

