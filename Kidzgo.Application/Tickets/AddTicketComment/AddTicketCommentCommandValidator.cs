using FluentValidation;

namespace Kidzgo.Application.Tickets.AddTicketComment;

public sealed class AddTicketCommentCommandValidator : AbstractValidator<AddTicketCommentCommand>
{
    public AddTicketCommentCommandValidator()
    {
        RuleFor(command => command.TicketId)
            .NotEmpty().WithMessage("Ticket ID is required");

        RuleFor(command => command.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(2000).WithMessage("Message must not exceed 2000 characters");
    }
}