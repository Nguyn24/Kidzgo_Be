using FluentValidation;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.Admin.UpdateUser;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        
        When(command => !string.IsNullOrWhiteSpace(command.Role), () =>
        {
            RuleFor(command => command.Role)
                .Must(role => Enum.TryParse<UserRole>(role, true, out _))
                .WithMessage("Role must be a valid value: Admin, Staff, Teacher, or Parent");
        });
        
        When(command => !string.IsNullOrWhiteSpace(command.Email), () =>
        {
            RuleFor(command => command.Email).EmailAddress();
        });
    }
}
