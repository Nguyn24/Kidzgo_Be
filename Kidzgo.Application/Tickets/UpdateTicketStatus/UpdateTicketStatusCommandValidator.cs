using FluentValidation;

namespace Kidzgo.Application.Tickets.UpdateTicketStatus;

public sealed class UpdateTicketStatusCommandValidator : AbstractValidator<UpdateTicketStatusCommand>
{
    public UpdateTicketStatusCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Ticket ID is required");

        RuleFor(command => command.Status)
            .IsInEnum().WithMessage("Invalid ticket status");
    }
}

