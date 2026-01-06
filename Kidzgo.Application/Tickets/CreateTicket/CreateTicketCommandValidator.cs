using FluentValidation;

namespace Kidzgo.Application.Tickets.CreateTicket;

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(command => command.OpenedByUserId)
            .NotEmpty().WithMessage("Opened By User ID is required");

        RuleFor(command => command.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleFor(command => command.Category)
            .IsInEnum().WithMessage("Invalid ticket category");

        RuleFor(command => command.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters");
    }
}

