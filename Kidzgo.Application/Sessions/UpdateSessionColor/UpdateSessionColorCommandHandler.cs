using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.UpdateSessionColor;

public sealed class UpdateSessionColorCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateSessionColorCommand>
{
    public async Task<Result> Handle(UpdateSessionColorCommand command, CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure(SessionErrors.NotFound(command.SessionId));
        }

        session.Color = command.Color.Trim().ToUpperInvariant();
        session.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
