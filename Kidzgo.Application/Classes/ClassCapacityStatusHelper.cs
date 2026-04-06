using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes;

internal static class ClassCapacityStatusHelper
{
    internal static void SyncAvailabilityStatus(Domain.Classes.Class classEntity, int activeEnrollmentCount, DateTime now)
    {
        if (classEntity.Status is ClassStatus.Completed or ClassStatus.Closed or ClassStatus.Cancelled or ClassStatus.Suspended)
        {
            return;
        }

        if (activeEnrollmentCount >= classEntity.Capacity)
        {
            if (classEntity.Status != ClassStatus.Full)
            {
                classEntity.Status = ClassStatus.Full;
                classEntity.UpdatedAt = now;
            }

            return;
        }

        if (classEntity.Status == ClassStatus.Full)
        {
            classEntity.Status = classEntity.StartDate <= DateOnly.FromDateTime(now)
                ? ClassStatus.Active
                : ClassStatus.Recruiting;
            classEntity.UpdatedAt = now;
        }
    }

    internal static async Task SyncAvailabilityStatusAsync(
        IDbContext context,
        Guid classId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);

        if (classEntity is null)
        {
            return;
        }

        var activeEnrollmentCount = classEntity.ClassEnrollments
            .Count(ce => ce.Status == EnrollmentStatus.Active);

        SyncAvailabilityStatus(classEntity, activeEnrollmentCount, now);
    }
}
