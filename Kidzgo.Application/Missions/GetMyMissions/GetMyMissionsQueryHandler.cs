using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.GetMyMissions;

public sealed class GetMyMissionsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetMyMissionsQuery, GetMyMissionsResponse>
{
    public async Task<Result<GetMyMissionsResponse>> Handle(
        GetMyMissionsQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.StudentId.HasValue)
        {
            return Result.Failure<GetMyMissionsResponse>(
                XpErrors.ProfileNotFound(Guid.Empty));
        }

        var studentProfileId = userContext.StudentId.Value;

        // Get all mission progress for this student
        var baseQuery = context.MissionProgresses
            .Include(mp => mp.Mission)
            .Where(mp => mp.StudentProfileId == studentProfileId);

        // Get total count
        int totalCount = await baseQuery.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var progresses = await baseQuery
            .OrderByDescending(mp => mp.Status == MissionProgressStatus.Completed)
            .ThenBy(mp => mp.Mission.EndAt ?? DateTime.MaxValue)
            .ThenByDescending(mp => mp.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(mp => new MyMissionProgressDto
            {
                Id = mp.Id,
                MissionId = mp.MissionId,
                Title = mp.Mission.Title,
                Description = mp.Mission.Description,
                MissionType = mp.Mission.MissionType.ToString(),
                ProgressMode = mp.Mission.ProgressMode.ToString(),
                Status = mp.Status.ToString(),
                ProgressValue = mp.ProgressValue,
                TotalRequired = mp.Mission.TotalRequired,
                ProgressPercentage = mp.Mission.TotalRequired.HasValue && mp.Mission.TotalRequired.Value > 0
                    ? (mp.ProgressValue ?? 0) * 100 / mp.Mission.TotalRequired.Value
                    : (mp.ProgressValue.HasValue ? 100m : 0m),
                RewardStars = mp.Mission.RewardStars,
                RewardExp = mp.Mission.RewardExp,
                StartAt = mp.Mission.StartAt,
                EndAt = mp.Mission.EndAt,
                CompletedAt = mp.CompletedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<MyMissionProgressDto>(
            progresses,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return Result.Success(new GetMyMissionsResponse
        {
            StudentProfileId = studentProfileId,
            Missions = page
        });
    }
}
