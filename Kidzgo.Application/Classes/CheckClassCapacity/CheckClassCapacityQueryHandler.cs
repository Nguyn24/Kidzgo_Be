using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.CheckClassCapacity;

public sealed class CheckClassCapacityQueryHandler(
    IDbContext context
) : IQueryHandler<CheckClassCapacityQuery, CheckClassCapacityResponse>
{
    public async Task<Result<CheckClassCapacityResponse>> Handle(CheckClassCapacityQuery query, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .Include(c => c.ClassEnrollments)
            .FirstOrDefaultAsync(c => c.Id == query.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<CheckClassCapacityResponse>(
                ClassErrors.NotFound(query.ClassId));
        }

        int currentEnrollmentCount = classEntity.ClassEnrollments
            .Count(ce => ce.Status == Domain.Classes.EnrollmentStatus.Active);

        int availableSlots = classEntity.Capacity - currentEnrollmentCount;
        bool hasAvailableSlots = availableSlots > 0;

        return new CheckClassCapacityResponse
        {
            ClassId = classEntity.Id,
            Capacity = classEntity.Capacity,
            CurrentEnrollmentCount = currentEnrollmentCount,
            AvailableSlots = availableSlots,
            HasAvailableSlots = hasAvailableSlots
        };
    }
}

