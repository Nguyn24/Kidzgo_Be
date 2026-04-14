using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.UpdateClassColor;

public sealed class UpdateClassColorCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateClassColorCommand>
{
    public async Task<Result> Handle(UpdateClassColorCommand command, CancellationToken cancellationToken)
    {
        var classExists = await context.Classes
            .AnyAsync(c => c.Id == command.ClassId, cancellationToken);

        if (!classExists)
        {
            return Result.Failure(ClassErrors.NotFound(command.ClassId));
        }

        var normalizedColor = command.Color.Trim().ToUpperInvariant();
        var now = VietnamTime.UtcNow();

        await context.Sessions
            .Where(s => s.ClassId == command.ClassId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Color, normalizedColor)
                .SetProperty(s => s.UpdatedAt, now), cancellationToken);

        return Result.Success();
    }
}
