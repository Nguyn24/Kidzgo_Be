using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.UpdatePlacementTest;

public sealed class UpdatePlacementTestCommandHandler(
    IDbContext context
) : ICommandHandler<UpdatePlacementTestCommand, UpdatePlacementTestResponse>
{
    public async Task<Result<UpdatePlacementTestResponse>> Handle(
        UpdatePlacementTestCommand command,
        CancellationToken cancellationToken)
    {
        // UC-029: Update Placement Test
        var placementTest = await context.PlacementTests
            .FirstOrDefaultAsync(pt => pt.Id == command.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<UpdatePlacementTestResponse>(
                PlacementTestErrors.NotFound(command.PlacementTestId));
        }

        // Cannot update completed test
        if (placementTest.Status == PlacementTestStatus.Completed)
        {
            return Result.Failure<UpdatePlacementTestResponse>(
                PlacementTestErrors.CannotUpdateCompletedTest);
        }

        // Validate StudentProfile if provided
        if (command.StudentProfileId.HasValue)
        {
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId.Value, cancellationToken);

            if (profile is null || profile.ProfileType != Kidzgo.Domain.Users.ProfileType.Student)
            {
                return Result.Failure<UpdatePlacementTestResponse>(
                    PlacementTestErrors.StudentProfileNotFound(command.StudentProfileId));
            }

            placementTest.StudentProfileId = command.StudentProfileId.Value;
        }

        // Validate Class if provided
        if (command.ClassId.HasValue)
        {
            var classExists = await context.Classes
                .AnyAsync(c => c.Id == command.ClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<UpdatePlacementTestResponse>(
                    PlacementTestErrors.ClassNotFound(command.ClassId));
            }

            placementTest.ClassId = command.ClassId.Value;
        }

        // Validate Invigilator if provided
        if (command.InvigilatorUserId.HasValue)
        {
            var invigilator = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.InvigilatorUserId.Value, cancellationToken);

            if (invigilator is null)
            {
                return Result.Failure<UpdatePlacementTestResponse>(
                    PlacementTestErrors.InvigilatorNotFound(command.InvigilatorUserId));
            }

            placementTest.InvigilatorUserId = command.InvigilatorUserId.Value;
        }

        // Update ScheduledAt if provided
        if (command.ScheduledAt.HasValue)
        {
            DateTime scheduledAtUtc = command.ScheduledAt.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(command.ScheduledAt.Value, DateTimeKind.Utc)
                : command.ScheduledAt.Value.ToUniversalTime();
            placementTest.ScheduledAt = scheduledAtUtc;
        }

        // Update Room if provided
        if (command.Room is not null)
        {
            placementTest.Room = string.IsNullOrWhiteSpace(command.Room) ? null : command.Room.Trim();
        }

        placementTest.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new UpdatePlacementTestResponse
        {
            Id = placementTest.Id,
            LeadId = placementTest.LeadId,
            StudentProfileId = placementTest.StudentProfileId,
            ClassId = placementTest.ClassId,
            ScheduledAt = placementTest.ScheduledAt,
            Status = placementTest.Status.ToString(),
            Room = placementTest.Room,
            InvigilatorUserId = placementTest.InvigilatorUserId,
            UpdatedAt = placementTest.UpdatedAt
        };
    }
}

