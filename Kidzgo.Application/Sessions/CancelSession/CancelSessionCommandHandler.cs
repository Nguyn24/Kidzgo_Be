using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.CancelSession;

public sealed class CancelSessionCommandHandler(
    IDbContext context,
    StudentSessionAssignmentService studentSessionAssignmentService
) : ICommandHandler<CancelSessionCommand>
{
    public async Task<Result> Handle(CancelSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(SessionErrors.NotFound(command.SessionId));
        }

        if (session.Status == SessionStatus.Cancelled)
        {
            return Result.Failure(SessionErrors.AlreadyCancelled);
        }

        session.Status = SessionStatus.Cancelled;
        session.UpdatedAt = VietnamTime.UtcNow();

        await studentSessionAssignmentService.SyncAssignmentsForSessionAsync(session, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}


