using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.CreateLeadChild;

public sealed class CreateLeadChildCommandHandler(
    IDbContext context
) : ICommandHandler<CreateLeadChildCommand, CreateLeadChildResponse>
{
    public async Task<Result<CreateLeadChildResponse>> Handle(
        CreateLeadChildCommand command,
        CancellationToken cancellationToken)
    {
        // Validate Lead exists
        var lead = await context.Leads
            .FirstOrDefaultAsync(l => l.Id == command.LeadId, cancellationToken);

        if (lead is null)
        {
            return Result.Failure<CreateLeadChildResponse>(
                LeadErrors.NotFound(command.LeadId));
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(command.ChildName))
        {
            return Result.Failure<CreateLeadChildResponse>(
                Domain.Common.Error.Validation("ChildName", "Child name is required"));
        }

        var now = DateTime.UtcNow;
        var leadChild = new LeadChild
        {
            Id = Guid.NewGuid(),
            LeadId = command.LeadId,
            ChildName = command.ChildName.Trim(),
            Dob = command.Dob.HasValue
                ? DateTime.SpecifyKind(command.Dob.Value.Date, DateTimeKind.Utc)
                : null,
            Gender = command.Gender?.Trim(),
            ProgramInterest = command.ProgramInterest?.Trim(),
            Notes = command.Notes?.Trim(),
            Status = LeadChildStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.LeadChildren.Add(leadChild);

        // Create activity for Lead
        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = command.LeadId,
            ActivityType = ActivityType.Note,
            Content = $"Child '{leadChild.ChildName}' added to lead",
            CreatedAt = now
        };

        context.LeadActivities.Add(activity);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateLeadChildResponse
        {
            Id = leadChild.Id,
            LeadId = leadChild.LeadId,
            ChildName = leadChild.ChildName,
            Dob = leadChild.Dob,
            Gender = leadChild.Gender,
            ProgramInterest = leadChild.ProgramInterest,
            Notes = leadChild.Notes,
            Status = leadChild.Status.ToString(),
            CreatedAt = leadChild.CreatedAt,
            UpdatedAt = leadChild.UpdatedAt
        });
    }
}

