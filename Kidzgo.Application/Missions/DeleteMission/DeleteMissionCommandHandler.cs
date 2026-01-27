using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.DeleteMission;

public sealed class DeleteMissionCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteMissionCommand>
{
    public async Task<Result> Handle(
        DeleteMissionCommand command,
        CancellationToken cancellationToken)
    {
        var mission = await context.Missions
            .Include(m => m.MissionProgresses)
            .FirstOrDefaultAsync(m => m.Id == command.Id, cancellationToken);

        if (mission is null)
        {
            return Result.Failure(
                MissionErrors.NotFound(command.Id));
        }

        // Check if mission has progress records
        if (mission.MissionProgresses.Any())
        {
            return Result.Failure(
                MissionErrors.MissionInUse);
        }

        context.Missions.Remove(mission);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

