using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Gamification;

namespace Kidzgo.Application.Missions.Shared;

public static class MissionProgressRewardHelper
{
    public static async Task ApplyProgressAsync(
        IGamificationService gamificationService,
        MissionProgress missionProgress,
        decimal progressValue,
        DateTime completedAt,
        string reasonPrefix,
        CancellationToken cancellationToken)
    {
        missionProgress.ProgressValue = progressValue;
        missionProgress.Status = progressValue > 0
            ? MissionProgressStatus.InProgress
            : MissionProgressStatus.Assigned;
        missionProgress.CompletedAt = null;

        var totalRequired = missionProgress.Mission.TotalRequired;
        if (!totalRequired.HasValue || totalRequired.Value <= 0 || progressValue < totalRequired.Value)
        {
            return;
        }

        missionProgress.Status = MissionProgressStatus.Completed;
        missionProgress.CompletedAt = completedAt;

        if (missionProgress.Mission.RewardStars.HasValue &&
            missionProgress.Mission.RewardStars.Value > 0)
        {
            await gamificationService.AddStarsForMissionCompletion(
                missionProgress.StudentProfileId,
                missionProgress.Mission.RewardStars.Value,
                missionProgress.MissionId,
                reason: $"Completed {reasonPrefix} Mission: {missionProgress.Mission.Title}",
                cancellationToken);
        }

        if (missionProgress.Mission.RewardExp.HasValue &&
            missionProgress.Mission.RewardExp.Value > 0)
        {
            await gamificationService.AddXpForMissionCompletion(
                missionProgress.StudentProfileId,
                missionProgress.Mission.RewardExp.Value,
                missionProgress.MissionId,
                reason: $"Completed {reasonPrefix} Mission: {missionProgress.Mission.Title}",
                cancellationToken);
        }
    }
}
