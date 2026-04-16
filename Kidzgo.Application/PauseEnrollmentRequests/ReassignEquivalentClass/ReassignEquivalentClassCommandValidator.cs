using FluentValidation;

namespace Kidzgo.Application.PauseEnrollmentRequests.ReassignEquivalentClass;

public sealed class ReassignEquivalentClassCommandValidator
    : AbstractValidator<ReassignEquivalentClassCommand>
{
    public ReassignEquivalentClassCommandValidator()
    {
        RuleFor(command => command.PauseEnrollmentRequestId)
            .NotEmpty()
            .WithMessage("PauseEnrollmentRequestId is required");

        RuleFor(command => command.RegistrationId)
            .NotEmpty()
            .WithMessage("RegistrationId is required");

        RuleFor(command => command.NewClassId)
            .NotEmpty()
            .WithMessage("NewClassId is required");
    }
}
