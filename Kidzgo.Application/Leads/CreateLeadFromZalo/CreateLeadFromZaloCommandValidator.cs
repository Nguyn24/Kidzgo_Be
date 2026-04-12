using FluentValidation;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Leads.CreateLeadFromZalo;

public sealed class CreateLeadFromZaloCommandValidator : AbstractValidator<CreateLeadFromZaloCommand>
{
    public CreateLeadFromZaloCommandValidator()
    {
        When(command => !string.IsNullOrWhiteSpace(command.Phone), () =>
        {
            RuleFor(command => command.Phone!)
                .MaximumLength(50)
                .Must(PhoneNumberNormalizer.IsValidVietnamesePhoneNumber)
                .WithMessage("Phone must be a valid 10-digit Vietnamese phone number");
        });
    }
}
