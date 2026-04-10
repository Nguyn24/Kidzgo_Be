using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.AddLeadNote;

public sealed class AddLeadNoteCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<AddLeadNoteCommand, AddLeadNoteResponse>
{
    public async Task<Result<AddLeadNoteResponse>> Handle(
        AddLeadNoteCommand command,
        CancellationToken cancellationToken)
    {
        var lead = await context.Leads
            .FirstOrDefaultAsync(l => l.Id == command.LeadId, cancellationToken);

        if (lead is null)
        {
            return Result.Failure<AddLeadNoteResponse>(
                LeadErrors.NotFound(command.LeadId));
        }

        var now = VietnamTime.UtcNow();
        var nextActionAtUtc = VietnamTime.NormalizeToUtc(command.NextActionAt);
        var isContactActivity = IsContactActivity(command.ActivityType);
        var shouldClearNextAction = command.ClearNextAction == true;
        var activityNextActionAt = shouldClearNextAction ? null : nextActionAtUtc;

        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = command.ActivityType,
            Content = command.Content.Trim(),
            NextActionAt = activityNextActionAt,
            CreatedBy = userContext.UserId,
            CreatedAt = now
        };

        context.LeadActivities.Add(activity);

        // First contact activities are the source of truth for SLA first response.
        if (isContactActivity && !lead.FirstResponseAt.HasValue)
        {
            lead.FirstResponseAt = now;
        }

        // Keep status progression aligned with actual outreach instead of assignment/data entry.
        if (isContactActivity && lead.Status == LeadStatus.New)
        {
            lead.Status = LeadStatus.Contacted;
        }

        // UC-025: Track touch count
        lead.TouchCount++;
        
        // UC-026: Keep the lead-level follow-up in sync with the requested action.
        if (shouldClearNextAction)
        {
            lead.NextActionAt = null;
        }
        else if (nextActionAtUtc.HasValue)
        {
            lead.NextActionAt = nextActionAtUtc.Value;
        }

        lead.UpdatedAt = now;
        await context.SaveChangesAsync(cancellationToken);

        return new AddLeadNoteResponse
        {
            ActivityId = activity.Id,
            LeadId = lead.Id,
            ActivityType = activity.ActivityType.ToString(),
            Content = activity.Content,
            LeadStatus = lead.Status.ToString(),
            FirstResponseAt = lead.FirstResponseAt,
            NextActionAt = activity.NextActionAt,
            LeadNextActionAt = lead.NextActionAt,
            CreatedAt = activity.CreatedAt
        };
    }

    private static bool IsContactActivity(ActivityType activityType)
    {
        return activityType is ActivityType.Call
            or ActivityType.Zalo
            or ActivityType.Sms
            or ActivityType.Email;
    }
}

