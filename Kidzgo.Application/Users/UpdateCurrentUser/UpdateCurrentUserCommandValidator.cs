using FluentValidation;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Users.UpdateCurrentUser;

public sealed class UpdateCurrentUserCommandValidator : AbstractValidator<UpdateCurrentUserCommand>
{
    public UpdateCurrentUserCommandValidator()
    {
        When(command => !string.IsNullOrWhiteSpace(command.Email), () =>
        {
            RuleFor(command => command.Email!)
                .EmailAddress();
        });

        When(command => !string.IsNullOrWhiteSpace(command.PhoneNumber), () =>
        {
            RuleFor(command => command.PhoneNumber!)
                .MaximumLength(50)
                .Must(PhoneNumberNormalizer.IsValidVietnamesePhoneNumber)
                .WithMessage("Phone number must be a valid Vietnamese phone number");
        });
    }
}
