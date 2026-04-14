using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.IncidentReports.UpdateIncidentReportStatus;

public sealed class UpdateIncidentReportStatusCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<UpdateIncidentReportStatusCommand, IncidentReportDto>
{
    public async Task<Result<IncidentReportDto>> Handle(UpdateIncidentReportStatusCommand command, CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<IncidentReportDto>(TicketErrors.UserNotFound);
        }

        if (!IncidentReportAccessPolicy.CanUpdateStatus(currentUser.Role))
        {
            return Result.Failure<IncidentReportDto>(IncidentReportErrors.UpdateStatusAdminOnly);
        }

        var ticket = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TicketComments)
            .FirstOrDefaultAsync(t => t.Id == command.Id && t.IsIncidentReport, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<IncidentReportDto>(IncidentReportErrors.NotFound(command.Id));
        }

        var currentStatus = ticket.IncidentStatus ?? IncidentReportStatus.Open;
        if (!IsValidTransition(currentStatus, command.Status))
        {
            return Result.Failure<IncidentReportDto>(
                IncidentReportErrors.InvalidStatusTransition(currentStatus.ToString(), command.Status.ToString()));
        }

        ticket.IncidentStatus = command.Status;
        ticket.Status = IncidentReportStateMapper.ToTicketStatus(command.Status);
        ticket.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return IncidentReportMapper.ToDto(ticket);
    }

    private static bool IsValidTransition(IncidentReportStatus current, IncidentReportStatus target)
    {
        if (current == target)
        {
            return false;
        }

        return current switch
        {
            IncidentReportStatus.Open => target is IncidentReportStatus.InProgress or IncidentReportStatus.Resolved or IncidentReportStatus.Closed or IncidentReportStatus.Rejected,
            IncidentReportStatus.InProgress => target is IncidentReportStatus.Resolved or IncidentReportStatus.Closed or IncidentReportStatus.Rejected,
            IncidentReportStatus.Resolved => target is IncidentReportStatus.Closed or IncidentReportStatus.Rejected,
            IncidentReportStatus.Closed => false,
            IncidentReportStatus.Rejected => false,
            _ => false
        };
    }
}
