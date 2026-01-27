using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.UpdateLeadStatus;

public sealed class UpdateLeadStatusCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateLeadStatusCommand, UpdateLeadStatusResponse>
{
    public async Task<Result<UpdateLeadStatusResponse>> Handle(
        UpdateLeadStatusCommand command,
        CancellationToken cancellationToken)
    {
        var lead = await context.Leads
            .FirstOrDefaultAsync(l => l.Id == command.LeadId, cancellationToken);

        if (lead is null)
        {
            return Result.Failure<UpdateLeadStatusResponse>(
                LeadErrors.NotFound(command.LeadId));
        }

        // Validate status transition (basic validation)
        if (lead.Status == LeadStatus.Enrolled && command.Status != LeadStatus.Enrolled)
        {
            return Result.Failure<UpdateLeadStatusResponse>(
                LeadErrors.InvalidStatusTransition);
        }

        var now = DateTime.UtcNow;
        var oldStatus = lead.Status;
        lead.Status = command.Status;
        lead.UpdatedAt = now;

        // UC-024: Track first response time when status changes to CONTACTED
        if (command.Status == LeadStatus.Contacted && !lead.FirstResponseAt.HasValue)
        {
            lead.FirstResponseAt = now;
        }

        // UC-025: Track touch count when status changes
        if (oldStatus != command.Status)
        {
            lead.TouchCount++;
        }

        // Create activity
        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = ActivityType.Note,
            Content = $"Status changed from {oldStatus} to {command.Status}",
            CreatedAt = now
        };

        context.LeadActivities.Add(activity);
        await context.SaveChangesAsync(cancellationToken);

        return new UpdateLeadStatusResponse
        {
            LeadId = lead.Id,
            Status = lead.Status.ToString(),
            FirstResponseAt = lead.FirstResponseAt
        };
    }
}

