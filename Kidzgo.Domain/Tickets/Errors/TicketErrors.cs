using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Tickets.Errors;

public static class TicketErrors
{
    public static Error NotFound(Guid? ticketId) => Error.NotFound(
        "Ticket.NotFound",
        $"Ticket with Id = '{ticketId}' was not found");

    public static readonly Error UserNotFound = Error.NotFound(
        "Ticket.UserNotFound",
        "User not found");

    public static readonly Error BranchNotFound = Error.NotFound(
        "Ticket.BranchNotFound",
        "Branch not found or inactive");

    public static readonly Error ClassNotFound = Error.NotFound(
        "Ticket.ClassNotFound",
        "Class not found");

    public static readonly Error ProfileNotFound = Error.NotFound(
        "Ticket.ProfileNotFound",
        "Profile not found or does not belong to the user");

    public static readonly Error AssignedUserNotFound = Error.NotFound(
        "Ticket.AssignedUserNotFound",
        "Assigned user not found or is not Staff/Teacher");

    public static readonly Error AssignedUserBranchMismatch = Error.Conflict(
        "Ticket.AssignedUserBranchMismatch",
        "Assigned user must belong to the same branch as the ticket");
}

public static class TicketCommentErrors
{
    public static readonly Error TicketNotFound = Error.NotFound(
        "TicketComment.TicketNotFound",
        "Ticket not found");

    public static readonly Error TicketClosed = Error.Conflict(
        "TicketComment.TicketClosed",
        "Cannot add comment to a closed ticket");

    public static readonly Error UserNotFound = Error.NotFound(
        "TicketComment.UserNotFound",
        "User not found");

    public static readonly Error ProfileNotFound = Error.NotFound(
        "TicketComment.ProfileNotFound",
        "Profile not found or does not belong to the user");
}

