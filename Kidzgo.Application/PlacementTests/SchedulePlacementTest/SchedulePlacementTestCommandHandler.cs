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
        // UC-027: Validate Lead exists
        var lead = await context.Leads
            .FirstOrDefaultAsync(l => l.Id == command.LeadId, cancellationToken);

        if (lead is null)
        {
            return Result.Failure<SchedulePlacementTestResponse>(
                PlacementTestErrors.LeadNotFound(command.LeadId));
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
            LeadId = command.LeadId,
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

        // Update Lead status to BOOKED_TEST
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

