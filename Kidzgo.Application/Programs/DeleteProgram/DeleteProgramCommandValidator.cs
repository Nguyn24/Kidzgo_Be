using FluentValidation;

namespace Kidzgo.Application.Programs.DeleteProgram;

public sealed class DeleteProgramCommandValidator : AbstractValidator<DeleteProgramCommand>
{
    public DeleteProgramCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Program ID is required");
    }
}

