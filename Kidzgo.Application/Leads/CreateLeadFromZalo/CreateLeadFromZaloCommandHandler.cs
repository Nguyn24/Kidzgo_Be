using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.CreateLeadFromZalo;

public sealed class CreateLeadFromZaloCommandHandler(
    IDbContext context
) : ICommandHandler<CreateLeadFromZaloCommand, CreateLeadFromZaloResponse>
{
    public async Task<Result<CreateLeadFromZaloResponse>> Handle(
        CreateLeadFromZaloCommand command,
        CancellationToken cancellationToken)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(command.ContactName) && 
            string.IsNullOrWhiteSpace(command.Phone) && 
            string.IsNullOrWhiteSpace(command.Email) &&
            string.IsNullOrWhiteSpace(command.ZaloUserId))
        {
            return Result.Failure<CreateLeadFromZaloResponse>(
                LeadErrors.InvalidContactInfo);
        }

        // Check if lead already exists (by phone, email, or zalo_id)
        var existingLead = await context.Leads
            .FirstOrDefaultAsync(l =>
                (!string.IsNullOrWhiteSpace(command.Phone) && l.Phone == command.Phone) ||
                (!string.IsNullOrWhiteSpace(command.Email) && l.Email == command.Email) ||
                (!string.IsNullOrWhiteSpace(command.ZaloUserId) && l.ZaloId == command.ZaloUserId),
                cancellationToken);

        if (existingLead is not null)
        {
            // Update existing lead instead of creating new one
            existingLead.TouchCount++;
            existingLead.UpdatedAt = DateTime.UtcNow;
            
            // Update contact info if provided
            if (!string.IsNullOrWhiteSpace(command.ContactName))
                existingLead.ContactName = command.ContactName;
            if (!string.IsNullOrWhiteSpace(command.Phone))
                existingLead.Phone = command.Phone;
            if (!string.IsNullOrWhiteSpace(command.Email))
                existingLead.Email = command.Email;
            if (!string.IsNullOrWhiteSpace(command.ZaloUserId))
                existingLead.ZaloId = command.ZaloUserId;

            // Update notes
            if (!string.IsNullOrWhiteSpace(command.Notes))
            {
                existingLead.Notes = string.IsNullOrWhiteSpace(existingLead.Notes)
                    ? command.Notes
                    : $"{existingLead.Notes}\n{command.Notes}";
            }

            await context.SaveChangesAsync(cancellationToken);

            return new CreateLeadFromZaloResponse
            {
                Id = existingLead.Id,
                IsNewLead = false,
                Message = "Lead updated (existing lead found)"
            };
        }

        // Validate branch if provided
        if (command.BranchPreference.HasValue)
        {
            var branchExists = await context.Branches
                .AnyAsync(b => b.Id == command.BranchPreference.Value, cancellationToken);

            if (!branchExists)
            {
                return Result.Failure<CreateLeadFromZaloResponse>(
                    LeadErrors.BranchNotFound);
            }
        }

        // Create new lead
        var now = DateTime.UtcNow;
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Source = LeadSource.Zalo,
            ContactName = command.ContactName,
            Phone = command.Phone,
            ZaloId = command.ZaloUserId,
            Email = command.Email,
            BranchPreference = command.BranchPreference,
            ProgramInterest = command.ProgramInterest,
            Notes = command.Notes ?? $"Created from Zalo. Message: {command.Message}",
            Status = LeadStatus.New,
            TouchCount = 1,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Leads.Add(lead);

        // Create Lead Activity để track
        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = ActivityType.Zalo,
            Content = $"Lead created from Zalo. Event: {command.ZaloOAId}",
            CreatedAt = now
        };

        context.LeadActivities.Add(activity);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateLeadFromZaloResponse
        {
            Id = lead.Id,
            IsNewLead = true,
            Message = "Lead created successfully from Zalo"
        };
    }
}

