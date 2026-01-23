using FluentValidation;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Username).NotNull().NotEmpty();
        RuleFor(command => command.Name).NotNull().NotEmpty();
        RuleFor(command => command.Email).NotNull().NotEmpty().EmailAddress();
        RuleFor(command => command.Password).NotNull().NotEmpty().MinimumLength(6);
        RuleFor(command => command.Role)
            .NotNull()
            .NotEmpty()
            .Must(role => Enum.TryParse<UserRole>(role, true, out _))
            .WithMessage("Role must be a valid value: Admin, ManagementStaff, AccountantStaff, Teacher, or Parent");

        When(command => Enum.TryParse<UserRole>(command.Role, true, out var role)
                        && (role == UserRole.ManagementStaff || role == UserRole.AccountantStaff || role == UserRole.Teacher || role == UserRole.Parent), () =>
        {
            RuleFor(command => command.BranchId)
                .NotNull()
                .WithMessage("BranchId is required for ManagementStaff, AccountantStaff, Teacher, and Parent accounts.");
        });

        When(command => !string.IsNullOrWhiteSpace(command.PhoneNumber), () =>
        {
            RuleFor(command => command.PhoneNumber).MaximumLength(50);
        });
    }
}
