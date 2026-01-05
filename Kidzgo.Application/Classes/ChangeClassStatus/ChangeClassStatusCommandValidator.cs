using FluentValidation;

namespace Kidzgo.Application.Classes.ChangeClassStatus;

public sealed class ChangeClassStatusCommandValidator : AbstractValidator<ChangeClassStatusCommand>
{
    public ChangeClassStatusCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Class ID is required");
    }
}

