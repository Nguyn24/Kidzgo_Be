using FluentValidation;

namespace Kidzgo.Application.Tickets.AssignTicket;

public sealed class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
{
    public AssignTicketCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Ticket ID is required");

        RuleFor(command => command.AssignedToUserId)
            .NotEmpty().WithMessage("Assigned To User ID is required");
    }
}

