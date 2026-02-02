using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.SchedulePlacementTest;

public sealed class SchedulePlacementTestCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<SchedulePlacementTestCommand, SchedulePlacementTestResponse>
{
    public async Task<Result<SchedulePlacementTestResponse>> Handle(
        SchedulePlacementTestCommand command,
        CancellationToken cancellationToken)
    {
        Lead? lead = null;
        LeadChild? leadChild = null;

        // UC-027: Priority 1 - If LeadChildId is provided, use it
        if (command.LeadChildId.HasValue)
        {
            leadChild = await context.LeadChildren
                .Include(lc => lc.Lead)
                .FirstOrDefaultAsync(lc => lc.Id == command.LeadChildId.Value, cancellationToken);

            if (leadChild is null)
            {
                return Result.Failure<SchedulePlacementTestResponse>(
                    Domain.Common.Error.NotFound("LeadChild", $"LeadChild with Id {command.LeadChildId.Value} not found"));
            }

            lead = leadChild.Lead;

            // Validate LeadId matches if provided
            if (command.LeadId.HasValue && command.LeadId.Value != lead.Id)
            {
                return Result.Failure<SchedulePlacementTestResponse>(
                    Domain.Common.Error.Validation("LeadId", "LeadId does not match LeadChild's LeadId"));
            }
        }
        // Priority 2 - If only LeadId is provided, use default LeadChild or create one
        else if (command.LeadId.HasValue)
        {
            lead = await context.Leads
                .Include(l => l.LeadChildren)
                .FirstOrDefaultAsync(l => l.Id == command.LeadId.Value, cancellationToken);

            if (lead is null)
            {
                return Result.Failure<SchedulePlacementTestResponse>(
                    PlacementTestErrors.LeadNotFound(command.LeadId.Value));
            }

            // Get or create default LeadChild
            leadChild = lead.LeadChildren.FirstOrDefault();
            if (leadChild is null)
            {
                // Create default LeadChild for backward compatibility
                var defaultNow = DateTime.UtcNow;
                leadChild = new LeadChild
                {
                    Id = Guid.NewGuid(),
                    LeadId = lead.Id,
                    ChildName = lead.ContactName ?? "Child",
                    Dob = null,
                    ProgramInterest = null,
                    Status = LeadChildStatus.New,
                    CreatedAt = defaultNow,
                    UpdatedAt = defaultNow
                };
                context.LeadChildren.Add(leadChild);
            }
        }
        else
        {
            return Result.Failure<SchedulePlacementTestResponse>(
                Domain.Common.Error.Validation("LeadId", "Either LeadId or LeadChildId must be provided"));
        }

        // Validate StudentProfile if provided
        if (command.StudentProfileId.HasValue)
        {
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId.Value, cancellationToken);

            if (profile is null || profile.ProfileType != Kidzgo.Domain.Users.ProfileType.Student)
            {
                return Result.Failure<SchedulePlacementTestResponse>(
                    PlacementTestErrors.StudentProfileNotFound(command.StudentProfileId));
            }
        }

        // Validate Class if provided
        if (command.ClassId.HasValue)
        {
            var classExists = await context.Classes
                .AnyAsync(c => c.Id == command.ClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<SchedulePlacementTestResponse>(
                    PlacementTestErrors.ClassNotFound(command.ClassId));
            }
        }

        // Validate Invigilator if provided
        if (command.InvigilatorUserId.HasValue)
        {
            var invigilator = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.InvigilatorUserId.Value, cancellationToken);

            if (invigilator is null)
            {
                return Result.Failure<SchedulePlacementTestResponse>(
                    PlacementTestErrors.InvigilatorNotFound(command.InvigilatorUserId));
            }
        }

        // Convert ScheduledAt to UTC
        DateTime scheduledAtUtc = command.ScheduledAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(command.ScheduledAt, DateTimeKind.Utc)
            : command.ScheduledAt.ToUniversalTime();

        var now = DateTime.UtcNow;
        var placementTest = new PlacementTest
        {
            Id = Guid.NewGuid(),
            LeadId = lead.Id,
            LeadChildId = leadChild.Id,
            StudentProfileId = command.StudentProfileId,
            ClassId = command.ClassId,
            ScheduledAt = scheduledAtUtc,
            Status = PlacementTestStatus.Scheduled,
            Room = command.Room?.Trim(),
            InvigilatorUserId = command.InvigilatorUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.PlacementTests.Add(placementTest);

        // Update LeadChild status to BookedTest
        if (leadChild.Status != LeadChildStatus.BookedTest)
        {
            leadChild.Status = LeadChildStatus.BookedTest;
            leadChild.UpdatedAt = now;
        }

        // Update Lead status to BOOKED_TEST (keep backward compatibility)
        if (lead.Status != LeadStatus.BookedTest)
        {
            lead.Status = LeadStatus.BookedTest;
            lead.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SchedulePlacementTestResponse
        {
            Id = placementTest.Id,
            LeadId = placementTest.LeadId,
            LeadChildId = placementTest.LeadChildId,
            StudentProfileId = placementTest.StudentProfileId,
            ClassId = placementTest.ClassId,
            ScheduledAt = placementTest.ScheduledAt,
            Status = placementTest.Status.ToString(),
            Room = placementTest.Room,
            InvigilatorUserId = placementTest.InvigilatorUserId,
            CreatedAt = placementTest.CreatedAt
        };
    }
}

