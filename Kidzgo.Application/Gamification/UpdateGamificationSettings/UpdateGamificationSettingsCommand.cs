using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Gamification.UpdateGamificationSettings;

public sealed class UpdateGamificationSettingsCommand : ICommand<UpdateGamificationSettingsResponse>
{
    public int CheckInRewardStars { get; init; }
    public int CheckInRewardExp { get; init; }
}
