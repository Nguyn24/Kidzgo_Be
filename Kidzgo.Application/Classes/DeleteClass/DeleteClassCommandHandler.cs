using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.DeleteClass;

public sealed class DeleteClassCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteClassCommand>
{
    public async Task<Result> Handle(DeleteClassCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure(
                ClassErrors.NotFound(command.Id));
        }

        // Check if class has active enrollments
        bool hasActiveEnrollments = await context.ClassEnrollments
            .AnyAsync(ce => ce.ClassId == command.Id && ce.Status == Domain.Classes.EnrollmentStatus.Active, cancellationToken);

        if (hasActiveEnrollments)
        {
            return Result.Failure(
                ClassErrors.HasActiveEnrollments);
        }

        // Soft delete: Set status to Closed
        classEntity.Status = ClassStatus.Closed;
        classEntity.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

