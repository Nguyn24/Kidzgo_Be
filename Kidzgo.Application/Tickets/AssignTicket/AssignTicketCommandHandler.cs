using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.AssignTicket;

public sealed class AssignTicketCommandHandler(
    IDbContext context
) : ICommandHandler<AssignTicketCommand, AssignTicketResponse>
{
    public async Task<Result<AssignTicketResponse>> Handle(AssignTicketCommand command, CancellationToken cancellationToken)
    {
        var ticket = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.OpenedByProfile)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<AssignTicketResponse>(
                Error.NotFound("Ticket.NotFound", "Ticket not found"));
        }

        // Check if assigned user exists and is Staff or Teacher
        var assignedUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.AssignedToUserId && 
                (u.Role == Domain.Users.UserRole.Staff || u.Role == Domain.Users.UserRole.Teacher), cancellationToken);

        if (assignedUser is null)
        {
            return Result.Failure<AssignTicketResponse>(
                Error.NotFound("Ticket.AssignedUserNotFound", "Assigned user not found or is not Staff/Teacher"));
        }

        // Check if assigned user belongs to the same branch as the ticket
        if (assignedUser.BranchId != ticket.BranchId)
        {
            return Result.Failure<AssignTicketResponse>(
                Error.Conflict("Ticket.AssignedUserBranchMismatch", "Assigned user must belong to the same branch as the ticket"));
        }

        ticket.AssignedToUserId = command.AssignedToUserId;
        
        // Auto-update status to InProgress if ticket is Open
        if (ticket.Status == Domain.Tickets.TicketStatus.Open)
        {
            ticket.Status = Domain.Tickets.TicketStatus.InProgress;
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        // Query ticket with navigation properties for response
        var ticketWithNav = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.OpenedByProfile)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == ticket.Id, cancellationToken);

        return new AssignTicketResponse
        {
            Id = ticketWithNav!.Id,
            OpenedByUserId = ticketWithNav.OpenedByUserId,
            OpenedByUserName = ticketWithNav.OpenedByUser.Name,
            OpenedByProfileId = ticketWithNav.OpenedByProfileId,
            OpenedByProfileName = ticketWithNav.OpenedByProfile?.DisplayName,
            BranchId = ticketWithNav.BranchId,
            BranchName = ticketWithNav.Branch.Name,
            ClassId = ticketWithNav.ClassId,
            ClassCode = ticketWithNav.Class?.Code,
            ClassTitle = ticketWithNav.Class?.Title,
            Category = ticketWithNav.Category,
            Message = ticketWithNav.Message,
            Status = ticketWithNav.Status,
            AssignedToUserId = ticketWithNav.AssignedToUserId,
            AssignedToUserName = ticketWithNav.AssignedToUser?.Name,
            CreatedAt = ticketWithNav.CreatedAt,
            UpdatedAt = ticketWithNav.UpdatedAt
        };
    }
}

