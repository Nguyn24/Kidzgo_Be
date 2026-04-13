using FluentValidation;

namespace Kidzgo.Application.Classes.UpdateClassColor;

public sealed class UpdateClassColorCommandValidator : AbstractValidator<UpdateClassColorCommand>
{
    public UpdateClassColorCommandValidator()
    {
        RuleFor(c => c.ClassId)
            .NotEmpty();

        RuleFor(c => c.Color)
            .NotEmpty()
            .WithMessage("Color is required")
            .Matches("^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be in format #RRGGBB");
    }
}
