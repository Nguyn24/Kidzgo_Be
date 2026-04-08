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
    public static async Task<Result> EnsureActorCanManageTargetsAsync(
        IDbContext context,
        Guid actorUserId,
        MissionScope scope,
        Guid? targetClassId,
        Guid? targetStudentId,
        List<Guid>? targetGroup,
        CancellationToken cancellationToken)
    {
        var actorRole = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == actorUserId)
            .Select(u => (UserRole?)u.Role)
            .FirstOrDefaultAsync(cancellationToken);

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
}
