using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.CompleteSession;

public sealed class CompleteSessionCommandHandler(
    IDbContext context
) : ICommandHandler<CompleteSessionCommand>
{
    public async Task<Result> Handle(CompleteSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(SessionErrors.NotFound(command.SessionId));
        }

        if (session.Status == SessionStatus.Cancelled)
        {
            return Result.Failure(
                Error.Validation("Session.Cancelled",
                    "Cancelled sessions cannot be completed"));
        }

        var actualUtc = command.ActualDatetime switch
        {
            null => DateTime.UtcNow,
            { Kind: DateTimeKind.Unspecified } dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
            var dt => dt.Value.ToUniversalTime()
        };

        session.Status = SessionStatus.Completed;
        session.ActualDatetime = actualUtc;
        session.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}



