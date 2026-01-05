using FluentValidation;

namespace Kidzgo.Application.Enrollments.UpdateEnrollment;

public sealed class UpdateEnrollmentCommandValidator : AbstractValidator<UpdateEnrollmentCommand>
{
    public UpdateEnrollmentCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Enrollment ID is required");
    }
}

