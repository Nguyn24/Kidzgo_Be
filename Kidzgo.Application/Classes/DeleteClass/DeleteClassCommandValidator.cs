using FluentValidation;

namespace Kidzgo.Application.Classes.DeleteClass;

public sealed class DeleteClassCommandValidator : AbstractValidator<DeleteClassCommand>
{
    public DeleteClassCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Class ID is required");
    }
}

