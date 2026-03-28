using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.CreateMission;

public sealed class CreateMissionCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateMissionCommand, CreateMissionResponse>
{
    public async Task<Result<CreateMissionResponse>> Handle(
        CreateMissionCommand command,
        CancellationToken cancellationToken)
    {
        // Validate scope requirements
        if (command.Scope == MissionScope.Class && !command.TargetClassId.HasValue)
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidScope);
        }

        if (command.Scope == MissionScope.Student && !command.TargetStudentId.HasValue)
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidScope);
        }

        if (command.Scope == MissionScope.Group &&
            (command.TargetGroup == null || command.TargetGroup.Count == 0))
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidScope);
        }

        // Validate date range
        if (command.StartAt.HasValue && command.EndAt.HasValue && command.EndAt.Value <= command.StartAt.Value)
        {
            return Result.Failure<CreateMissionResponse>(
                MissionErrors.InvalidDateRange);
        }

        // Verify class exists if scope is Class
        if (command.Scope == MissionScope.Class && command.TargetClassId.HasValue)
        {
            var classExists = await context.Classes
                .AnyAsync(c => c.Id == command.TargetClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<CreateMissionResponse>(
                    MissionErrors.ClassNotFound);
            }
        }

        // Verify student profile exists if scope is Student
        if (command.Scope == MissionScope.Student && command.TargetStudentId.HasValue)
        {
            var studentExists = await context.Profiles
                .AnyAsync(p => p.Id == command.TargetStudentId.Value, cancellationToken);

            if (!studentExists)
            {
                return Result.Failure<CreateMissionResponse>(
                    MissionErrors.StudentNotFound);
            }
        }

        // Verify student profiles exist if scope is Group
        if (command.Scope == MissionScope.Group && command.TargetGroup != null && command.TargetGroup.Count > 0)
        {
            var existingStudentIds = await context.Profiles
                .Where(p => command.TargetGroup.Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var missingIds = command.TargetGroup.Except(existingStudentIds).ToList();
            if (missingIds.Count > 0)
            {
                return Result.Failure<CreateMissionResponse>(
                    MissionErrors.SomeStudentsNotFound(missingIds.Count));
            }
        }

        var now = DateTime.UtcNow;
        var userId = userContext.UserId;

        // Convert DateTime to UTC if provided
        DateTime? startAtUtc = command.StartAt.HasValue
            ? command.StartAt.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(command.StartAt.Value, DateTimeKind.Utc)
                : command.StartAt.Value.ToUniversalTime()
            : null;

        DateTime? endAtUtc = command.EndAt.HasValue
            ? command.EndAt.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(command.EndAt.Value, DateTimeKind.Utc)
                : command.EndAt.Value.ToUniversalTime()
            : null;

        var mission = new Mission
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Description = command.Description,
            Scope = command.Scope,
            TargetClassId = command.TargetClassId,
            TargetStudentId = command.TargetStudentId,
            TargetGroup = command.TargetGroup,
            MissionType = command.MissionType,
            StartAt = startAtUtc,
            EndAt = endAtUtc,
            RewardStars = command.RewardStars,
            RewardExp = command.RewardExp,
            TotalRequired = command.TotalRequired,
            CreatedBy = userId,
            CreatedAt = now
        };

        context.Missions.Add(mission);

        // ============================================================
        // GIAI DOAN 1: Auto-create MissionProgress for all target students
        // ============================================================
        List<Guid> targetStudentIds = [];

        switch (command.Scope)
        {
            case MissionScope.Class when command.TargetClassId.HasValue:
                // Lay danh sach student profiles dang ACTIVE trong lop
                targetStudentIds = await context.ClassEnrollments
                    .Where(e => e.ClassId == command.TargetClassId.Value && e.Status == EnrollmentStatus.Active)
                    .Select(e => e.StudentProfileId)
                    .ToListAsync(cancellationToken);
                break;

            case MissionScope.Student when command.TargetStudentId.HasValue:
                targetStudentIds = [command.TargetStudentId.Value];
                break;

            case MissionScope.Group when command.TargetGroup != null && command.TargetGroup.Count > 0:
                targetStudentIds = command.TargetGroup;
                break;
        }

        // Tao MissionProgress cho moi hoc sinh
        foreach (var studentProfileId in targetStudentIds)
        {
            var missionProgress = new MissionProgress
            {
                Id = Guid.NewGuid(),
                MissionId = mission.Id,
                StudentProfileId = studentProfileId,
                Status = MissionProgressStatus.Assigned,
                ProgressValue = 0,
                CompletedAt = null,
                VerifiedBy = null
            };

            context.MissionProgresses.Add(missionProgress);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CreateMissionResponse
        {
            Id = mission.Id,
            Title = mission.Title,
            Description = mission.Description,
            Scope = mission.Scope.ToString(),
            TargetClassId = mission.TargetClassId,
            TargetStudentId = mission.TargetStudentId,
            TargetGroup = mission.TargetGroup,
            MissionType = mission.MissionType.ToString(),
            StartAt = mission.StartAt,
            EndAt = mission.EndAt,
            RewardStars = mission.RewardStars,
            RewardExp = mission.RewardExp,
            TotalRequired = mission.TotalRequired,
            CreatedBy = mission.CreatedBy,
            CreatedAt = mission.CreatedAt
        };
    }
}
