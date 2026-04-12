using FluentValidation;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Leads.UpdateLead;

public sealed class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
{
    public UpdateLeadCommandValidator()
    {
        RuleFor(command => command.LeadId).NotEmpty();

        When(command => !string.IsNullOrWhiteSpace(command.Phone), () =>
        {
            RuleFor(command => command.Phone!)
                .MaximumLength(50)
                .Must(PhoneNumberNormalizer.IsValidVietnamesePhoneNumber)
                .WithMessage("Phone must be a valid 10-digit Vietnamese phone number");
        });
    }
}
