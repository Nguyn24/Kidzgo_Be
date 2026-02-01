using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
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

        ticket.Status = command.Status;
        ticket.UpdatedAt = DateTime.UtcNow;
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
}

