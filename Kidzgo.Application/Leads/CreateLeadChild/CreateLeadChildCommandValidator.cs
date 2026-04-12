using FluentValidation;
using Kidzgo.Application.Leads;

namespace Kidzgo.Application.Leads.CreateLeadChild;

public sealed class CreateLeadChildCommandValidator : AbstractValidator<CreateLeadChildCommand>
{
    public CreateLeadChildCommandValidator()
    {
        RuleFor(command => command.LeadId)
            .NotEmpty()
            .WithMessage("LeadId is required");

        RuleFor(command => command.ChildName)
            .NotEmpty()
            .WithMessage("Child name is required");

        RuleFor(command => command.Dob)
            .MustBeValidLeadChildDob();
    }
}
