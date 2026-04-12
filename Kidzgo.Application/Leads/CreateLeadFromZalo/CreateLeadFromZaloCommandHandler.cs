using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Users;
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
        var normalizedPhone = PhoneNumberNormalizer.NormalizeVietnamesePhoneNumber(command.Phone);
        var phoneLookupCandidates = PhoneNumberNormalizer.GetLookupCandidates(command.Phone);

        // Validate required fields
        if (string.IsNullOrWhiteSpace(command.ContactName) && 
            normalizedPhone is null && 
            string.IsNullOrWhiteSpace(command.Email) &&
            string.IsNullOrWhiteSpace(command.ZaloUserId))
        {
            return Result.Failure<CreateLeadFromZaloResponse>(
                LeadErrors.InvalidContactInfo);
        }

        // Check if lead already exists (by phone, email, or zalo_id)
        var existingLead = await context.Leads
            .FirstOrDefaultAsync(l =>
                (phoneLookupCandidates.Length > 0 &&
                 l.Phone != null &&
                 phoneLookupCandidates.Contains(l.Phone)) ||
                (!string.IsNullOrWhiteSpace(command.Email) && l.Email == command.Email) ||
                (!string.IsNullOrWhiteSpace(command.ZaloUserId) && l.ZaloId == command.ZaloUserId),
                cancellationToken);

        if (existingLead is not null)
        {
            // Update existing lead instead of creating new one
            existingLead.TouchCount++;
            existingLead.UpdatedAt = VietnamTime.UtcNow();
            
            // Update contact info if provided
            if (!string.IsNullOrWhiteSpace(command.ContactName))
                existingLead.ContactName = command.ContactName;
            if (normalizedPhone is not null)
                existingLead.Phone = normalizedPhone;
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
        var now = VietnamTime.UtcNow();
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Source = LeadSource.Zalo,
            ContactName = command.ContactName,
            Phone = normalizedPhone,
            ZaloId = command.ZaloUserId,
            Email = command.Email,
            BranchPreference = command.BranchPreference,
            Notes = command.Notes ?? $"Created from Zalo. Message: {command.Message}",
            Status = LeadStatus.New,
            TouchCount = 1,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Leads.Add(lead);

        // Create default LeadChild (since Lead no longer stores child info)
        var leadChild = new LeadChild
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ChildName = command.ContactName, // best-effort default
            Dob = null,
            Gender = Gender.Male,
            ProgramInterest = string.IsNullOrWhiteSpace(command.ProgramInterest) ? null : command.ProgramInterest.Trim(),
            Notes = null,
            Status = LeadChildStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.LeadChildren.Add(leadChild);

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

