using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.IncidentReports.AssignIncidentReport;

public sealed class AssignIncidentReportCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<AssignIncidentReportCommand, IncidentReportDto>
{
    public async Task<Result<IncidentReportDto>> Handle(AssignIncidentReportCommand command, CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<IncidentReportDto>(TicketErrors.UserNotFound);
        }

        if (!IncidentReportAccessPolicy.CanAssign(currentUser.Role))
        {
            return Result.Failure<IncidentReportDto>(IncidentReportErrors.AssignAdminOnly);
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

        var assignedUser = await context.Users
            .FirstOrDefaultAsync(
                u => u.Id == command.AssignedToUserId && IncidentReportAccessPolicy.IsValidAssigneeRole(u.Role),
                cancellationToken);

        if (assignedUser is null)
        {
            return Result.Failure<IncidentReportDto>(IncidentReportErrors.AssignedUserNotFound);
        }

        if (assignedUser.Role != Domain.Users.UserRole.Admin && assignedUser.BranchId != ticket.BranchId)
        {
            return Result.Failure<IncidentReportDto>(IncidentReportErrors.AssignedUserBranchMismatch);
        }

        ticket.AssignedToUserId = command.AssignedToUserId;

        if (ticket.IncidentStatus == IncidentReportStatus.Open)
        {
            ticket.IncidentStatus = IncidentReportStatus.InProgress;
            ticket.Status = IncidentReportStateMapper.ToTicketStatus(IncidentReportStatus.InProgress);
        }

        ticket.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return IncidentReportMapper.ToDto(ticket);
    }
}
