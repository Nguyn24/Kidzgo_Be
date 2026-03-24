using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.LinkHomeworkToMission;

public sealed class LinkHomeworkToMissionCommandHandler(
    IDbContext context
) : ICommandHandler<LinkHomeworkToMissionCommand, LinkHomeworkToMissionResponse>
{
    public async Task<Result<LinkHomeworkToMissionResponse>> Handle(
        LinkHomeworkToMissionCommand command,
        CancellationToken cancellationToken)
    {
        var homework = await context.HomeworkAssignments
            .FirstOrDefaultAsync(h => h.Id == command.HomeworkId, cancellationToken);

        if (homework is null)
        {
            return Result.Failure<LinkHomeworkToMissionResponse>(
                HomeworkErrors.NotFound(command.HomeworkId));
        }

        var mission = await context.Missions
            .FirstOrDefaultAsync(m => m.Id == command.MissionId, cancellationToken);

        if (mission is null)
        {
            return Result.Failure<LinkHomeworkToMissionResponse>(
                HomeworkErrors.MissionNotFound(command.MissionId));
        }

        homework.MissionId = command.MissionId;

        await context.SaveChangesAsync(cancellationToken);

        return new LinkHomeworkToMissionResponse
        {
            HomeworkId = homework.Id,
            MissionId = mission.Id,
            MissionTitle = mission.Title
        };
    }
}

