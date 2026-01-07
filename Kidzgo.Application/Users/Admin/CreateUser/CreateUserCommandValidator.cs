using FluentValidation;

namespace Kidzgo.Application.Users.Admin.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Name).NotNull().NotEmpty();
        RuleFor(command => command.Email).NotNull().NotEmpty().EmailAddress();
        RuleFor(command => command.Password).NotNull().NotEmpty().MinimumLength(6);
        // Note: IsInEnum() is enough - don't use NotEmpty() because Admin = 0 (default enum value)
        RuleFor(command => command.Role).IsInEnum();
    }
}