using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.SetHomeworkRewardStars;

public sealed class SetHomeworkRewardStarsCommandHandler(
    IDbContext context
) : ICommandHandler<SetHomeworkRewardStarsCommand, SetHomeworkRewardStarsResponse>
{
    public async Task<Result<SetHomeworkRewardStarsResponse>> Handle(
        SetHomeworkRewardStarsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.RewardStars < 0)
        {
            return Result.Failure<SetHomeworkRewardStarsResponse>(
                HomeworkErrors.InvalidRewardStars);
        }

        var homework = await context.HomeworkAssignments
            .FirstOrDefaultAsync(h => h.Id == command.HomeworkId, cancellationToken);

        if (homework is null)
        {
            return Result.Failure<SetHomeworkRewardStarsResponse>(
                HomeworkErrors.NotFound(command.HomeworkId));
        }

        homework.RewardStars = command.RewardStars;

        await context.SaveChangesAsync(cancellationToken);

        return new SetHomeworkRewardStarsResponse
        {
            HomeworkId = homework.Id,
            RewardStars = homework.RewardStars ?? 0
        };
    }
}

