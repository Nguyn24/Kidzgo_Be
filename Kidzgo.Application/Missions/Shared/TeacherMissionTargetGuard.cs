using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.Shared;

internal static class TeacherMissionTargetGuard
{
    public static async Task<IQueryable<Mission>> FilterReadableMissionsForActorAsync(
        IDbContext context,
        Guid actorUserId,
        IQueryable<Mission> missionsQuery,
        CancellationToken cancellationToken)
    {
        var actorRole = await GetActorRoleAsync(context, actorUserId, cancellationToken);

        return actorRole == UserRole.Teacher
            ? ApplyTeacherReadableMissionFilter(context, actorUserId, missionsQuery)
            : missionsQuery;
    }

    public static async Task<Result> EnsureActorCanReadMissionAsync(
        IDbContext context,
        Guid actorUserId,
        Guid missionId,
        CancellationToken cancellationToken)
    {
        var actorRole = await GetActorRoleAsync(context, actorUserId, cancellationToken);

        if (actorRole != UserRole.Teacher)
        {
            return Result.Success();
        }

        var canRead = await ApplyTeacherReadableMissionFilter(
                context,
                actorUserId,
                context.Missions.AsNoTracking().Where(m => m.Id == missionId))
            .AnyAsync(cancellationToken);

        return canRead
            ? Result.Success()
            : Result.Failure(MissionErrors.TeacherCannotViewMission);
    }

    public static async Task<Result> EnsureActorCanManageTargetsAsync(
        IDbContext context,
        Guid actorUserId,
        MissionScope scope,
        Guid? targetClassId,
        Guid? targetStudentId,
        List<Guid>? targetGroup,
        CancellationToken cancellationToken)
    {
        var actorRole = await GetActorRoleAsync(context, actorUserId, cancellationToken);

        if (actorRole != UserRole.Teacher)
        {
            return Result.Success();
        }

        switch (scope)
        {
            case MissionScope.Class when targetClassId.HasValue:
            {
                var canAccessClass = await context.Classes
                    .AsNoTracking()
                    .AnyAsync(c =>
                        c.Id == targetClassId.Value &&
                        (c.MainTeacherId == actorUserId || c.AssistantTeacherId == actorUserId),
                        cancellationToken);

                return canAccessClass
                    ? Result.Success()
                    : Result.Failure(MissionErrors.TeacherCannotTargetClass);
            }

            case MissionScope.Student when targetStudentId.HasValue:
            {
                var canAccessStudent = await context.ClassEnrollments
                    .AsNoTracking()
                    .AnyAsync(e =>
                        e.StudentProfileId == targetStudentId.Value &&
                        e.Status == EnrollmentStatus.Active &&
                        (e.Class.MainTeacherId == actorUserId || e.Class.AssistantTeacherId == actorUserId),
                        cancellationToken);

                return canAccessStudent
                    ? Result.Success()
                    : Result.Failure(MissionErrors.TeacherCannotTargetStudent);
            }

            case MissionScope.Group when targetGroup is { Count: > 0 }:
            {
                var distinctTargetIds = targetGroup.Distinct().ToList();

                var accessibleStudentIds = await context.ClassEnrollments
                    .AsNoTracking()
                    .Where(e =>
                        distinctTargetIds.Contains(e.StudentProfileId) &&
                        e.Status == EnrollmentStatus.Active &&
                        (e.Class.MainTeacherId == actorUserId || e.Class.AssistantTeacherId == actorUserId))
                    .Select(e => e.StudentProfileId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var unauthorizedCount = distinctTargetIds.Except(accessibleStudentIds).Count();

                return unauthorizedCount == 0
                    ? Result.Success()
                    : Result.Failure(MissionErrors.TeacherCannotTargetSomeStudents(unauthorizedCount));
            }

            default:
                return Result.Success();
        }
    }

    private static Task<UserRole?> GetActorRoleAsync(
        IDbContext context,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        return context.Users
            .AsNoTracking()
            .Where(u => u.Id == actorUserId)
            .Select(u => (UserRole?)u.Role)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<Mission> ApplyTeacherReadableMissionFilter(
        IDbContext context,
        Guid teacherUserId,
        IQueryable<Mission> missionsQuery)
    {
        var taughtClassIds = context.Classes
            .AsNoTracking()
            .Where(c => c.MainTeacherId == teacherUserId || c.AssistantTeacherId == teacherUserId)
            .Select(c => c.Id);

        var taughtStudentIds = context.ClassEnrollments
            .AsNoTracking()
            .Where(e => e.Status == EnrollmentStatus.Active && taughtClassIds.Contains(e.ClassId))
            .Select(e => e.StudentProfileId)
            .Distinct();

        return missionsQuery.Where(m =>
            (m.Scope == MissionScope.Class &&
             m.TargetClassId.HasValue &&
             taughtClassIds.Contains(m.TargetClassId.Value)) ||
            (m.Scope == MissionScope.Student &&
             m.TargetStudentId.HasValue &&
             taughtStudentIds.Contains(m.TargetStudentId.Value)) ||
            (m.Scope == MissionScope.Group &&
             m.MissionProgresses.Any() &&
             m.MissionProgresses.All(mp => taughtStudentIds.Contains(mp.StudentProfileId))));
    }
}
