using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.ChangeClassStatus;

public sealed class ChangeClassStatusCommandHandler(
    IDbContext context
) : ICommandHandler<ChangeClassStatusCommand, ChangeClassStatusResponse>
{
    public async Task<Result<ChangeClassStatusResponse>> Handle(ChangeClassStatusCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<ChangeClassStatusResponse>(
                Error.NotFound("Class.NotFound", "Class not found"));
        }

        // Validate status transition
        if (classEntity.Status == command.Status)
        {
            return Result.Failure<ChangeClassStatusResponse>(
                Error.Validation("Class.StatusUnchanged", "Class status is already set to the requested status"));
        }

        // Business rules for status transitions
        // PLANNED -> ACTIVE: OK
        // ACTIVE -> CLOSED: OK
        // CLOSED -> ACTIVE: OK (reopen)
        // CLOSED -> PLANNED: Not allowed
        if (classEntity.Status == Domain.Classes.ClassStatus.Closed && command.Status == Domain.Classes.ClassStatus.Planned)
        {
            return Result.Failure<ChangeClassStatusResponse>(
                Error.Validation("Class.InvalidStatusTransition", "Cannot change status from Closed to Planned"));
        }

        classEntity.Status = command.Status;
        classEntity.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new ChangeClassStatusResponse
        {
            Id = classEntity.Id,
            Status = classEntity.Status
        };
    }
}

