using FluentValidation;

namespace Kidzgo.Application.Enrollments.DropEnrollment;

public sealed class DropEnrollmentCommandValidator : AbstractValidator<DropEnrollmentCommand>
{
    public DropEnrollmentCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Enrollment ID is required");
    }
}

