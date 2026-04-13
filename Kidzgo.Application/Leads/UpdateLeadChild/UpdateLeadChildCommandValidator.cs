using FluentValidation;
using Kidzgo.Application.Leads;

namespace Kidzgo.Application.Leads.UpdateLeadChild;

public sealed class UpdateLeadChildCommandValidator : AbstractValidator<UpdateLeadChildCommand>
{
    public UpdateLeadChildCommandValidator()
    {
        RuleFor(command => command.LeadId)
            .NotEmpty()
            .WithMessage("LeadId is required");

        RuleFor(command => command.ChildId)
            .NotEmpty()
            .WithMessage("ChildId is required");

        When(command => command.ChildName is not null, () =>
        {
            RuleFor(command => command.ChildName!)
                .NotEmpty()
                .WithMessage("Child name cannot be empty");
        });

        RuleFor(command => command.Dob)
            .MustBeValidLeadChildDob();
    }
}
