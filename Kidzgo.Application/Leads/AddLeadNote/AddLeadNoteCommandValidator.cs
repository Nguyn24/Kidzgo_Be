using FluentValidation;

namespace Kidzgo.Application.Leads.AddLeadNote;

public sealed class AddLeadNoteCommandValidator : AbstractValidator<AddLeadNoteCommand>
{
    public AddLeadNoteCommandValidator()
    {
        RuleFor(command => command.LeadId)
            .NotEmpty()
            .WithMessage("Lead ID is required");

        RuleFor(command => command.Content)
            .NotEmpty()
            .WithMessage("Content is required");

        RuleFor(command => command.NextActionAt)
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("NextActionAt should not be in the past")
            .When(command => command.NextActionAt.HasValue);

        RuleFor(command => command)
            .Must(command => command.ClearNextAction != true || !command.NextActionAt.HasValue)
            .WithMessage("Cannot set NextActionAt and ClearNextAction at the same time");
    }
}

