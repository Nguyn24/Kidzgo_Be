using FluentValidation;

namespace Kidzgo.Application.Authentication.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email).NotNull().NotEmpty().WithMessage("Email is required");
        RuleFor(command => command.Password).NotNull().NotEmpty().WithMessage("Password is required");
    }
}