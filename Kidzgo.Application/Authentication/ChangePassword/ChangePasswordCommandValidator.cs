using FluentValidation;

namespace Kidzgo.Application.Authentication.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(command => command.CurrentPassword).NotNull().NotEmpty();
            RuleFor(command => command.NewPassword).NotNull().NotEmpty();
        }
    }
}
