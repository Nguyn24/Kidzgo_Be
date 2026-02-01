using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.SetHomeworkRewardStars;

public sealed class SetHomeworkRewardStarsCommand : ICommand<SetHomeworkRewardStarsResponse>
{
    public Guid HomeworkId { get; init; }
    public int RewardStars { get; init; }
}

