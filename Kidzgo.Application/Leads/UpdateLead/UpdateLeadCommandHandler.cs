using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.UpdateLead;

public sealed class UpdateLeadCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateLeadCommand, UpdateLeadResponse>
{
    public async Task<Result<UpdateLeadResponse>> Handle(
        UpdateLeadCommand command,
        CancellationToken cancellationToken)
    {
        var lead = await context.Leads
            .FirstOrDefaultAsync(l => l.Id == command.LeadId, cancellationToken);

        if (lead is null)
        {
            return Result.Failure<UpdateLeadResponse>(
                LeadErrors.NotFound(command.LeadId));
        }

        // Cannot update converted lead
        if (lead.Status == Kidzgo.Domain.CRM.LeadStatus.Enrolled)
        {
            return Result.Failure<UpdateLeadResponse>(
                LeadErrors.CannotUpdateConvertedLead);
        }

        // Validate branch if provided
        if (command.BranchPreference.HasValue)
        {
            var branchExists = await context.Branches
                .AnyAsync(b => b.Id == command.BranchPreference.Value, cancellationToken);

            if (!branchExists)
            {
                return Result.Failure<UpdateLeadResponse>(
                    LeadErrors.BranchNotFound);
            }
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(command.ContactName))
        {
            lead.ContactName = command.ContactName.Trim();
        }

        if (command.ChildName != null)
        {
            lead.ChildName = string.IsNullOrWhiteSpace(command.ChildName)
                ? null
                : command.ChildName.Trim();
        }

        if (command.ChildDateOfBirth.HasValue)
        {
            lead.ChildDateOfBirth = DateTime.SpecifyKind(
                command.ChildDateOfBirth.Value.Date,
                DateTimeKind.Utc);
        }

        if (command.Phone != null)
        {
            lead.Phone = string.IsNullOrWhiteSpace(command.Phone) ? null : command.Phone.Trim();
        }

        if (command.ZaloId != null)
        {
            lead.ZaloId = string.IsNullOrWhiteSpace(command.ZaloId) ? null : command.ZaloId.Trim();
        }

        if (command.Email != null)
        {
            lead.Email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim();
        }

        if (command.Company != null)
        {
            lead.Company = string.IsNullOrWhiteSpace(command.Company) ? null : command.Company.Trim();
        }

        if (command.Subject != null)
        {
            lead.Subject = string.IsNullOrWhiteSpace(command.Subject) ? null : command.Subject.Trim();
        }

        if (command.BranchPreference.HasValue)
        {
            lead.BranchPreference = command.BranchPreference.Value;
        }

        if (command.ProgramInterest != null)
        {
            lead.ProgramInterest = string.IsNullOrWhiteSpace(command.ProgramInterest)
                ? null
                : command.ProgramInterest.Trim();
        }

        if (command.Notes != null)
        {
            lead.Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim();
        }

        lead.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateLeadResponse
        {
            Id = lead.Id,
            ContactName = lead.ContactName,
            ChildName = lead.ChildName,
            ChildDateOfBirth = lead.ChildDateOfBirth,
            Phone = lead.Phone,
            ZaloId = lead.ZaloId,
            Email = lead.Email,
            Company = lead.Company,
            Subject = lead.Subject,
            BranchPreference = lead.BranchPreference,
            ProgramInterest = lead.ProgramInterest,
            Notes = lead.Notes,
            UpdatedAt = lead.UpdatedAt
        };
    }
}

