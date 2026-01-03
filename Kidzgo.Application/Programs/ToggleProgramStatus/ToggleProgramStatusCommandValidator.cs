using FluentValidation;

namespace Kidzgo.Application.Programs.ToggleProgramStatus;

public sealed class ToggleProgramStatusCommandValidator : AbstractValidator<ToggleProgramStatusCommand>
{
    public ToggleProgramStatusCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Program ID is required");
    }
}

