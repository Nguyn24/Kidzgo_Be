using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets;

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

    public static Error InvalidStatusTransition(TicketStatus currentStatus, TicketStatus newStatus) => Error.Conflict(
        "Ticket.InvalidStatusTransition",
        $"Cannot change status from {currentStatus} to {newStatus}. Status can only move forward: Open → InProgress → Resolved → Closed");

    public static readonly Error ClassIdRequiredForDirectToTeacher = Error.Validation(
        "Ticket.ClassIdRequiredForDirectToTeacher",
        "ClassId is required when creating a DirectToTeacher ticket");

    public static readonly Error AssignedUserRequiredForDirectToTeacher = Error.Validation(
        "Ticket.AssignedUserRequiredForDirectToTeacher",
        "AssignedToUserId is required when creating a DirectToTeacher ticket");

    public static readonly Error MustBeMainTeacherOfClass = Error.Validation(
        "Ticket.MustBeMainTeacherOfClass",
        "The assigned user must be the MainTeacher of the class");

    public static readonly Error NotEnrolledInClass = Error.Validation(
        "Ticket.NotEnrolledInClass",
        "You must be enrolled in the class to create a DirectToTeacher ticket");
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
