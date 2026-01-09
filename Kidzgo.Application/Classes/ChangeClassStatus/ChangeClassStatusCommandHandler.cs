using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes.Errors;
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
                ClassErrors.NotFound(command.Id));
        }

        // Validate status transition
        if (classEntity.Status == command.Status)
        {
            return Result.Failure<ChangeClassStatusResponse>(
                ClassErrors.StatusUnchanged);
        }

        // Business rules for status transitions
        // PLANNED -> ACTIVE: OK
        // ACTIVE -> CLOSED: OK
        // CLOSED -> ACTIVE: OK (reopen)
        // CLOSED -> PLANNED: Not allowed
        if (classEntity.Status == Domain.Classes.ClassStatus.Closed && command.Status == Domain.Classes.ClassStatus.Planned)
        {
            return Result.Failure<ChangeClassStatusResponse>(
                ClassErrors.InvalidStatusTransition);
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

