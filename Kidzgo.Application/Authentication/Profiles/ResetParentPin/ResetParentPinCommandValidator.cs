using FluentValidation;

namespace Kidzgo.Application.Authentication.Profiles.ResetParentPin;

public sealed class ResetParentPinCommandValidator : AbstractValidator<ResetParentPinCommand>
{
    public ResetParentPinCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.NewPin)
            .NotEmpty();
    }
}
