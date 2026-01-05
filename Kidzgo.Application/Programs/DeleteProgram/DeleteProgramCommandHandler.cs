using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.DeleteProgram;

public sealed class DeleteProgramCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteProgramCommand>
{
    public async Task<Result> Handle(DeleteProgramCommand command, CancellationToken cancellationToken)
    {
        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.Id && !p.IsDeleted, cancellationToken);

        if (program is null)
        {
            return Result.Failure(
                Error.NotFound("Program.NotFound", "Program not found"));
        }

        // Check if program is being used by any active classes
        bool hasActiveClasses = await context.Classes
            .AnyAsync(c => c.ProgramId == program.Id && c.Status == ClassStatus.Active, cancellationToken);

        if (hasActiveClasses)
        {
            return Result.Failure(
                Error.Conflict("Program.HasActiveClasses", "Cannot delete program with active classes"));
        }

        // Soft delete
        program.IsDeleted = true;
        program.IsActive = false; // Deactivate when soft deleting
        program.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

