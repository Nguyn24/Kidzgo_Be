using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.AssignLead;

public sealed class AssignLeadCommandHandler(
    IDbContext context
) : ICommandHandler<AssignLeadCommand, AssignLeadResponse>
{
    public async Task<Result<AssignLeadResponse>> Handle(
        AssignLeadCommand command,
        CancellationToken cancellationToken)
    {
        var lead = await context.Leads
            .FirstOrDefaultAsync(l => l.Id == command.LeadId, cancellationToken);

        if (lead is null)
        {
            return Result.Failure<AssignLeadResponse>(
                LeadErrors.NotFound(command.LeadId));
        }

        var owner = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.OwnerStaffId, cancellationToken);

        if (owner is null)
        {
            return Result.Failure<AssignLeadResponse>(
                LeadErrors.OwnerNotFound(command.OwnerStaffId));
        }

        if (owner.Role != UserRole.ManagementStaff && owner.Role != UserRole.AccountantStaff)
        {
            return Result.Failure<AssignLeadResponse>(
                LeadErrors.OwnerNotStaff);
        }

        lead.OwnerStaffId = command.OwnerStaffId;
        lead.UpdatedAt = DateTime.UtcNow;

        // Track touch count
        lead.TouchCount++;

        // Create activity
        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = ActivityType.Note,
            Content = $"Lead assigned to {owner.Name ?? owner.Email}",
            CreatedAt = DateTime.UtcNow
        };

        context.LeadActivities.Add(activity);
        await context.SaveChangesAsync(cancellationToken);

        return new AssignLeadResponse
        {
            LeadId = lead.Id,
            OwnerStaffId = owner.Id,
            OwnerStaffName = owner.Name ?? owner.Email
        };
    }
}

