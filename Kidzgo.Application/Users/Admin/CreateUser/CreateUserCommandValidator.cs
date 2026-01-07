using FluentValidation;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Name).NotNull().NotEmpty();
        RuleFor(command => command.Email).NotNull().NotEmpty().EmailAddress();
        RuleFor(command => command.Password).NotNull().NotEmpty().MinimumLength(6);
        RuleFor(command => command.Role)
            .IsInEnum()
            .WithMessage("Role must be a valid value: Admin, Staff, Teacher, Student, or Parent");
    }
}