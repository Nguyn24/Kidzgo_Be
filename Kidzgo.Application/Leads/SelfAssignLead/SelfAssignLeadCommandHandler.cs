using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.SelfAssignLead;

public sealed class SelfAssignLeadCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<SelfAssignLeadCommand, SelfAssignLeadResponse>
{
    public async Task<Result<SelfAssignLeadResponse>> Handle(
        SelfAssignLeadCommand command,
        CancellationToken cancellationToken)
    {
        var lead = await context.Leads
            .FirstOrDefaultAsync(l => l.Id == command.LeadId, cancellationToken);

        if (lead is null)
        {
            return Result.Failure<SelfAssignLeadResponse>(
                LeadErrors.NotFound(command.LeadId));
        }

        var currentUserId = userContext.UserId;

        var owner = await context.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);

        if (owner is null)
        {
            return Result.Failure<SelfAssignLeadResponse>(
                LeadErrors.OwnerNotFound(currentUserId));
        }

        if (owner.Role != UserRole.ManagementStaff)
        {
            // Chỉ cho phép ManagementStaff tự nhận lead
            return Result.Failure<SelfAssignLeadResponse>(
                LeadErrors.OwnerNotStaff);
        }

        var now = VietnamTime.UtcNow();

        lead.OwnerStaffId = currentUserId;
        lead.UpdatedAt = now;

        // Tăng touch count
        lead.TouchCount++;

        // Activity log
        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = ActivityType.Note,
            Content = $"Lead self-assigned by {owner.Name ?? owner.Email}",
            CreatedAt = now
        };

        context.LeadActivities.Add(activity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new SelfAssignLeadResponse
        {
            LeadId = lead.Id,
            OwnerStaffId = owner.Id,
            OwnerStaffName = owner.Name ?? owner.Email
        });
    }
}


