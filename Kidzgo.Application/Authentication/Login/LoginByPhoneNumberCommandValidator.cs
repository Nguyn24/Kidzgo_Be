using FluentValidation;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Authentication.Login;

public sealed class LoginByPhoneNumberCommandValidator : AbstractValidator<LoginByPhoneNumberCommand>
{
    public LoginByPhoneNumberCommandValidator()
    {
        RuleFor(command => command.PhoneNumber)
            .NotNull()
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Must(PhoneNumberNormalizer.IsValidVietnamesePhoneNumber)
            .WithMessage("Phone number must be a valid 10-digit Vietnamese phone number");
    }
}
