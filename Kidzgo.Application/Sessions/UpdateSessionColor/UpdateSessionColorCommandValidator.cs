using FluentValidation;

namespace Kidzgo.Application.Sessions.UpdateSessionColor;

public sealed class UpdateSessionColorCommandValidator : AbstractValidator<UpdateSessionColorCommand>
{
    public UpdateSessionColorCommandValidator()
    {
        RuleFor(c => c.SessionId)
            .NotEmpty();

        RuleFor(c => c.Color)
            .NotEmpty()
            .WithMessage("Color is required")
            .Matches("^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be in format #RRGGBB");
    }
}
