using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.RetakePlacementTest;

public sealed class RetakePlacementTestCommandHandler(
    IDbContext context
) : ICommandHandler<RetakePlacementTestCommand, RetakePlacementTestResponse>
{
    public async Task<Result<RetakePlacementTestResponse>> Handle(
        RetakePlacementTestCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var originalPlacementTest = await context.PlacementTests
            .Include(pt => pt.Lead)
            .FirstOrDefaultAsync(pt => pt.Id == command.OriginalPlacementTestId, cancellationToken);

        if (originalPlacementTest is null)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                PlacementTestErrors.NotFound(command.OriginalPlacementTestId));
        }

        var student = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId
                && p.ProfileType == Kidzgo.Domain.Users.ProfileType.Student, cancellationToken);

        if (student is null)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                PlacementTestErrors.StudentProfileNotFound(command.StudentProfileId));
        }

        if (originalPlacementTest.StudentProfileId != command.StudentProfileId)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                PlacementTestErrors.StudentProfileNotFound(command.StudentProfileId));
        }

        var existingRetake = await context.PlacementTests
            .AnyAsync(pt =>
                pt.StudentProfileId == command.StudentProfileId &&
                pt.Id != command.OriginalPlacementTestId &&
                (pt.Status == PlacementTestStatus.Scheduled ||
                 pt.Status == PlacementTestStatus.Completed),
                cancellationToken);

        if (existingRetake)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                PlacementTestErrors.RetakeAlreadyScheduled(command.StudentProfileId));
        }

        var activeRegistration = await context.Registrations
            .Include(r => r.Program)
            .Include(r => r.TuitionPlan)
            .FirstOrDefaultAsync(r =>
                r.StudentProfileId == command.StudentProfileId &&
                r.Status != RegistrationStatus.Completed &&
                r.Status != RegistrationStatus.Cancelled,
                cancellationToken);

        var originalProgramName = activeRegistration?.Program?.Name;
        var originalTuitionPlanName = activeRegistration?.TuitionPlan?.Name;
        var originalRemainingSessions = activeRegistration?.RemainingSessions ?? 0;

        var branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                RegistrationErrors.BranchNotFound(command.BranchId));
        }

        if (command.InvigilatorUserId.HasValue)
        {
            var invigilatorExists = await context.Users
                .AnyAsync(u => u.Id == command.InvigilatorUserId.Value, cancellationToken);

            if (!invigilatorExists)
            {
                return Result.Failure<RetakePlacementTestResponse>(
                    PlacementTestErrors.InvigilatorNotFound(command.InvigilatorUserId.Value));
            }
        }

        var newProgram = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.NewProgramId && p.IsActive && !p.IsDeleted, cancellationToken);

        if (newProgram is null)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                RegistrationErrors.ProgramNotFound(command.NewProgramId));
        }

        var newTuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(tp =>
                tp.Id == command.NewTuitionPlanId &&
                tp.ProgramId == command.NewProgramId &&
                tp.IsActive, cancellationToken);

        if (newTuitionPlan is null)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                RegistrationErrors.TuitionPlanNotFound(command.NewTuitionPlanId));
        }

        var newPlacementTest = new PlacementTest
        {
            Id = Guid.NewGuid(),
            LeadId = originalPlacementTest.LeadId,
            LeadChildId = originalPlacementTest.LeadChildId,
            StudentProfileId = command.StudentProfileId,
            ClassId = null,
            ScheduledAt = command.ScheduledAt,
            Status = PlacementTestStatus.Scheduled,
            Room = command.Room,
            InvigilatorUserId = command.InvigilatorUserId,
            OriginalPlacementTestId = command.OriginalPlacementTestId,
            ResultScore = null,
            ListeningScore = null,
            SpeakingScore = null,
            ReadingScore = null,
            WritingScore = null,
            LevelRecommendation = null,
            ProgramRecommendation = newProgram.Name,
            Notes = command.Note ?? $"Retake from PlacementTest {originalPlacementTest.Id}. Target program: {newProgram.Name}.",
            AttachmentUrl = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.PlacementTests.Add(newPlacementTest);

        if (originalPlacementTest.LeadId.HasValue)
        {
            var scheduledAtStr = command.ScheduledAt?.ToString("yyyy-MM-dd HH:mm") ?? "TBD";
            context.LeadActivities.Add(new LeadActivity
            {
                Id = Guid.NewGuid(),
                LeadId = originalPlacementTest.LeadId.Value,
                ActivityType = ActivityType.Note,
                Content = $"Student retake placement test scheduled. Original program: '{(originalProgramName ?? "N/A")}'. Target program after retake: '{newProgram.Name}'. Scheduled at: {scheduledAtStr}.",
                CreatedAt = now
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return new RetakePlacementTestResponse
        {
            NewPlacementTestId = newPlacementTest.Id,
            OriginalPlacementTestId = originalPlacementTest.Id,
            StudentProfileId = command.StudentProfileId,
            OriginalProgramName = originalProgramName,
            NewProgramName = newProgram.Name,
            OriginalTuitionPlanName = originalTuitionPlanName,
            NewTuitionPlanName = newTuitionPlan.Name,
            OriginalRemainingSessions = originalRemainingSessions,
            PlacementTestStatus = newPlacementTest.Status.ToString(),
            ScheduledAt = newPlacementTest.ScheduledAt,
            Room = newPlacementTest.Room,
            InvigilatorUserId = newPlacementTest.InvigilatorUserId,
            CreatedAt = now
        };
    }
}
