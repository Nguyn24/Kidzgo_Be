using FluentValidation;

namespace Kidzgo.Application.Users.Admin.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Name).NotNull().NotEmpty();
        RuleFor(command => command.Email).NotNull().NotEmpty();
        RuleFor(command => command.Role).IsInEnum().NotNull().NotEmpty();
    }
}