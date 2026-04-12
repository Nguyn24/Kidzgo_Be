using FluentValidation;
using Kidzgo.Application.Leads;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Leads.CreateLead;

public sealed class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        When(command => !string.IsNullOrWhiteSpace(command.Phone), () =>
        {
            RuleFor(command => command.Phone!)
                .MaximumLength(50)
                .Must(PhoneNumberNormalizer.IsValidVietnamesePhoneNumber)
                .WithMessage("Phone must be a valid 10-digit Vietnamese phone number");
        });

        RuleForEach(command => command.Children)
            .SetValidator(new CreateLeadChildDtoValidator());
    }
}

internal sealed class CreateLeadChildDtoValidator : AbstractValidator<CreateLeadChildDto>
{
    public CreateLeadChildDtoValidator()
    {
        RuleFor(child => child.ChildName)
            .NotEmpty()
            .WithMessage("Child name is required");

        RuleFor(child => child.Dob)
            .MustBeValidLeadChildDob();
    }
}
