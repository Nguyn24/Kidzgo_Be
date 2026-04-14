using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.IncidentReports.CreateIncidentReport;

public sealed class CreateIncidentReportCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateIncidentReportCommand, IncidentReportDetailDto>
{
    public async Task<Result<IncidentReportDetailDto>> Handle(CreateIncidentReportCommand command, CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<IncidentReportDetailDto>(TicketErrors.UserNotFound);
        }

        if (!IncidentReportAccessPolicy.CanUseIncidentFlow(currentUser.Role))
        {
            return Result.Failure<IncidentReportDetailDto>(IncidentReportErrors.InvalidRole);
        }

        var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (branch is null)
        {
            return Result.Failure<IncidentReportDetailDto>(TicketErrors.BranchNotFound);
        }

        Class? classEntity = null;
        if (command.ClassId.HasValue)
        {
            classEntity = await context.Classes
                .FirstOrDefaultAsync(c => c.Id == command.ClassId.Value, cancellationToken);

            if (classEntity is null)
            {
                return Result.Failure<IncidentReportDetailDto>(TicketErrors.ClassNotFound);
            }
        }

        var now = VietnamTime.UtcNow();
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            OpenedByUserId = currentUser.Id,
            BranchId = command.BranchId,
            ClassId = command.ClassId,
            Category = TicketCategory.Other,
            Subject = command.Subject,
            Message = command.Message,
            Status = TicketStatus.Open,
            Type = TicketType.General,
            CreatedAt = now,
            UpdatedAt = now,
            IsIncidentReport = true,
            IncidentCategory = command.Category,
            IncidentStatus = IncidentReportStatus.Open,
            IncidentEvidenceUrl = command.EvidenceUrl
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync(cancellationToken);

        var created = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TicketComments)
            .FirstAsync(t => t.Id == ticket.Id, cancellationToken);

        return IncidentReportMapper.ToDetailDto(created);
    }
}
