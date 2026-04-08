using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.UpdateTicketStatus;

public sealed class UpdateTicketStatusCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateTicketStatusCommand, UpdateTicketStatusResponse>
{
    public async Task<Result<UpdateTicketStatusResponse>> Handle(UpdateTicketStatusCommand command, CancellationToken cancellationToken)
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
            return Result.Failure<UpdateTicketStatusResponse>(TicketErrors.NotFound(command.Id));
        }

        // Validate status transition: can only move forward (Open → InProgress → Resolved → Closed)
        var currentStatus = ticket.Status;
        var newStatus = command.Status;

        if (!IsValidStatusTransition(currentStatus, newStatus))
        {
            return Result.Failure<UpdateTicketStatusResponse>(
                TicketErrors.InvalidStatusTransition(currentStatus, newStatus));
        }

        ticket.Status = command.Status;
        ticket.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return new UpdateTicketStatusResponse
        {
            Id = ticket.Id,
            OpenedByUserId = ticket.OpenedByUserId,
            OpenedByUserName = ticket.OpenedByUser.Name,
            OpenedByProfileId = ticket.OpenedByProfileId,
            OpenedByProfileName = ticket.OpenedByProfile?.DisplayName,
            BranchId = ticket.BranchId,
            BranchName = ticket.Branch.Name,
            ClassId = ticket.ClassId,
            ClassCode = ticket.Class?.Code,
            ClassTitle = ticket.Class?.Title,
            Category = ticket.Category.ToString(),
            Subject = ticket.Subject,
            Message = ticket.Message,
            Status = ticket.Status.ToString(),
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = ticket.AssignedToUser?.Name,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };
    }

    private static bool IsValidStatusTransition(TicketStatus current, TicketStatus target)
    {
        // Define the allowed transitions in order
        var statusOrder = new[]
        {
            TicketStatus.Open,
            TicketStatus.InProgress,
            TicketStatus.Resolved,
            TicketStatus.Closed
        };

        var currentIndex = Array.IndexOf(statusOrder, current);
        var targetIndex = Array.IndexOf(statusOrder, target);

        // Both statuses must be valid
        if (currentIndex < 0 || targetIndex < 0)
            return false;

        // Can only move forward (target index must be greater than current index)
        return targetIndex > currentIndex;
    }
}
