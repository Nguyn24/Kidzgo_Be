using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.ToggleProgramStatus;

public sealed class ToggleProgramStatusCommandHandler(
    IDbContext context
) : ICommandHandler<ToggleProgramStatusCommand, ToggleProgramStatusResponse>
{
    public async Task<Result<ToggleProgramStatusResponse>> Handle(ToggleProgramStatusCommand command, CancellationToken cancellationToken)
    {
        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.Id && !p.IsDeleted, cancellationToken);

        if (program is null)
        {
            return Result.Failure<ToggleProgramStatusResponse>(ProgramErrors.NotFound(command.Id));
        }

        program.IsActive = !program.IsActive;
        program.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return new ToggleProgramStatusResponse
        {
            Id = program.Id,
            IsActive = program.IsActive
        };
    }
}

