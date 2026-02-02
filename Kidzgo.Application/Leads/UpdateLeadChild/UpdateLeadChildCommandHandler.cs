using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.UpdateLeadChild;

public sealed class UpdateLeadChildCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateLeadChildCommand, UpdateLeadChildResponse>
{
    public async Task<Result<UpdateLeadChildResponse>> Handle(
        UpdateLeadChildCommand command,
        CancellationToken cancellationToken)
    {
        // Validate Lead exists
        var leadExists = await context.Leads
            .AnyAsync(l => l.Id == command.LeadId, cancellationToken);

        if (!leadExists)
        {
            return Result.Failure<UpdateLeadChildResponse>(
                LeadErrors.NotFound(command.LeadId));
        }

        // Get LeadChild
        var leadChild = await context.LeadChildren
            .FirstOrDefaultAsync(lc => lc.Id == command.ChildId && lc.LeadId == command.LeadId, cancellationToken);

        if (leadChild is null)
        {
            return Result.Failure<UpdateLeadChildResponse>(
                Domain.Common.Error.NotFound("LeadChild", $"LeadChild with Id {command.ChildId} not found"));
        }

        // Update fields if provided
        if (command.ChildName is not null)
        {
            leadChild.ChildName = command.ChildName.Trim();
        }

        if (command.Dob.HasValue)
        {
            leadChild.Dob = DateTime.SpecifyKind(command.Dob.Value.Date, DateTimeKind.Utc);
        }
        else if (command.Dob == null && command.Dob.HasValue == false)
        {
            // Explicit null - clear the value
            leadChild.Dob = null;
        }

        if (command.Gender is not null)
        {
            leadChild.Gender = string.IsNullOrWhiteSpace(command.Gender) ? null : command.Gender.Trim();
        }

        if (command.ProgramInterest is not null)
        {
            leadChild.ProgramInterest = string.IsNullOrWhiteSpace(command.ProgramInterest) ? null : command.ProgramInterest.Trim();
        }

        if (command.Notes is not null)
        {
            leadChild.Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim();
        }

        leadChild.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateLeadChildResponse
        {
            Id = leadChild.Id,
            LeadId = leadChild.LeadId,
            ChildName = leadChild.ChildName,
            Dob = leadChild.Dob,
            Gender = leadChild.Gender,
            ProgramInterest = leadChild.ProgramInterest,
            Notes = leadChild.Notes,
            Status = leadChild.Status.ToString(),
            UpdatedAt = leadChild.UpdatedAt
        });
    }
}

