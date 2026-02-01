using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Leads.CreateLead;

public sealed class CreateLeadCommandHandler(
    IDbContext context
) : ICommandHandler<CreateLeadCommand, CreateLeadResponse>
{
    public async Task<Result<CreateLeadResponse>> Handle(
        CreateLeadCommand command,
        CancellationToken cancellationToken)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(command.ContactName) &&
            string.IsNullOrWhiteSpace(command.Phone) &&
            string.IsNullOrWhiteSpace(command.Email) &&
            string.IsNullOrWhiteSpace(command.ZaloId))
        {
            return Result.Failure<CreateLeadResponse>(
                LeadErrors.InvalidContactInfo);
        }

        // Check for duplicate lead (by phone, email, or zalo_id)
        var duplicateLead = await context.Leads
            .FirstOrDefaultAsync(l =>
                (!string.IsNullOrWhiteSpace(command.Phone) && l.Phone == command.Phone) ||
                (!string.IsNullOrWhiteSpace(command.Email) && l.Email == command.Email) ||
                (!string.IsNullOrWhiteSpace(command.ZaloId) && l.ZaloId == command.ZaloId),
                cancellationToken);

        if (duplicateLead is not null)
        {
            return Result.Failure<CreateLeadResponse>(
                LeadErrors.DuplicateLead);
        }

        // Validate branch if provided
        if (command.BranchPreference.HasValue)
        {
            var branchExists = await context.Branches
                .AnyAsync(b => b.Id == command.BranchPreference.Value, cancellationToken);

            if (!branchExists)
            {
                return Result.Failure<CreateLeadResponse>(
                    LeadErrors.BranchNotFound);
            }
        }

        // Validate owner staff if provided
        if (command.OwnerStaffId.HasValue)
        {
            var owner = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.OwnerStaffId.Value, cancellationToken);

            if (owner is null)
            {
                return Result.Failure<CreateLeadResponse>(
                    LeadErrors.OwnerNotFound(command.OwnerStaffId));
            }

            if (owner.Role != UserRole.ManagementStaff && owner.Role != UserRole.AccountantStaff)
            {
                return Result.Failure<CreateLeadResponse>(
                    LeadErrors.OwnerNotStaff);
            }
        }

        var now = DateTime.UtcNow;
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Source = command.Source,
            Campaign = command.Campaign?.Trim(),
            ContactName = command.ContactName.Trim(),
            ChildName = command.ChildName?.Trim(),
            ChildDateOfBirth = command.ChildDateOfBirth.HasValue
                ? DateTime.SpecifyKind(command.ChildDateOfBirth.Value.Date, DateTimeKind.Utc)
                : null,
            Phone = command.Phone?.Trim(),
            ZaloId = command.ZaloId?.Trim(),
            Email = command.Email?.Trim(),
            Company = command.Company?.Trim(),
            Subject = command.Subject?.Trim(),
            BranchPreference = command.BranchPreference,
            ProgramInterest = command.ProgramInterest?.Trim(),
            Notes = command.Notes?.Trim(),
            Status = LeadStatus.New,
            OwnerStaffId = command.OwnerStaffId,
            TouchCount = 1,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Leads.Add(lead);

        // Create initial activity
        var activity = new LeadActivity
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            ActivityType = ActivityType.Note,
            Content = $"Lead created from {command.Source}",
            CreatedAt = now
        };

        context.LeadActivities.Add(activity);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateLeadResponse
        {
            Id = lead.Id,
            Source = lead.Source.ToString(),
            Campaign = lead.Campaign,
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
            Status = lead.Status.ToString(),
            OwnerStaffId = lead.OwnerStaffId,
            CreatedAt = lead.CreatedAt
        };
    }
}

