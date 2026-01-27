using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.GetMissionProgress;

public sealed class GetMissionProgressQueryHandler(
    IDbContext context
) : IQueryHandler<GetMissionProgressQuery, GetMissionProgressResponse>
{
    public async Task<Result<GetMissionProgressResponse>> Handle(
        GetMissionProgressQuery query,
        CancellationToken cancellationToken)
    {
        // Get mission info
        var mission = await context.Missions
            .FirstOrDefaultAsync(m => m.Id == query.MissionId, cancellationToken);

        if (mission is null)
        {
            return Result.Failure<GetMissionProgressResponse>(
                MissionErrors.NotFound(query.MissionId));
        }

        // Get progress records
        var progressQuery = context.MissionProgresses
            .Include(mp => mp.StudentProfile)
            .Include(mp => mp.VerifiedByUser)
            .Where(mp => mp.MissionId == query.MissionId)
            .AsQueryable();

        // Filter by student if provided
        if (query.StudentProfileId.HasValue)
        {
            progressQuery = progressQuery.Where(mp => mp.StudentProfileId == query.StudentProfileId.Value);
        }

        // Get total count
        int totalCount = await progressQuery.CountAsync(cancellationToken);

        // Apply pagination
        var progresses = await progressQuery
            .OrderByDescending(mp => mp.CompletedAt)
            .ThenBy(mp => mp.StudentProfile.DisplayName)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(mp => new MissionProgressDto
            {
                Id = mp.Id,
                MissionId = mp.MissionId,
                StudentProfileId = mp.StudentProfileId,
                StudentName = mp.StudentProfile.DisplayName,
                Status = mp.Status.ToString(),
                ProgressValue = mp.ProgressValue,
                ProgressPercentage = CalculateProgressPercentage(mp.ProgressValue),
                CompletedAt = mp.CompletedAt,
                VerifiedBy = mp.VerifiedBy,
                VerifiedByName = mp.VerifiedByUser != null ? mp.VerifiedByUser.Name : null
            })
            .ToListAsync(cancellationToken);

        var page = new Page<MissionProgressDto>(
            progresses,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetMissionProgressResponse
        {
            Mission = new MissionProgressInfoDto
            {
                Id = mission.Id,
                Title = mission.Title
            },
            Progresses = page
        };
    }

    private static decimal CalculateProgressPercentage(decimal? progressValue)
    {
        if (!progressValue.HasValue)
        {
            return 0;
        }

        // progressValue is already a percentage (0-100)
        // If it's > 100, cap it at 100
        return Math.Min(100, Math.Max(0, progressValue.Value));
    }
}

