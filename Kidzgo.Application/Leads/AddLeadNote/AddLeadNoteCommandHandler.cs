using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
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

        var now = DateTime.UtcNow;
        
        // Convert NextActionAt to UTC if provided
        DateTime? nextActionAtUtc = command.NextActionAt.HasValue
            ? command.NextActionAt.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(command.NextActionAt.Value, DateTimeKind.Utc)
                : command.NextActionAt.Value.ToUniversalTime()
            : null;
        
        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = command.ActivityType,
            Content = command.Content.Trim(),
            NextActionAt = nextActionAtUtc,
            CreatedBy = userContext.UserId,
            CreatedAt = now
        };

        context.LeadActivities.Add(activity);

        // UC-025: Track touch count
        lead.TouchCount++;
        
        // UC-026: Set next action date if provided
        if (nextActionAtUtc.HasValue)
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
            NextActionAt = activity.NextActionAt,
            CreatedAt = activity.CreatedAt
        };
    }
}

