using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Missions.GetMissions;

public sealed class GetMissionsQueryHandler(
    IDbContext context
) : IQueryHandler<GetMissionsQuery, GetMissionsResponse>
{
    public async Task<Result<GetMissionsResponse>> Handle(
        GetMissionsQuery query,
        CancellationToken cancellationToken)
    {
        var missionsQuery = context.Missions
            .Include(m => m.TargetClass)
            .Include(m => m.CreatedByUser)
            .AsQueryable();

        // Filter by scope
        if (query.Scope.HasValue)
        {
            missionsQuery = missionsQuery.Where(m => m.Scope == query.Scope.Value);
        }

        // Filter by target class
        if (query.TargetClassId.HasValue)
        {
            missionsQuery = missionsQuery.Where(m => m.TargetClassId == query.TargetClassId.Value);
        }

        // Filter by target group
        if (!string.IsNullOrWhiteSpace(query.TargetGroup))
        {
            missionsQuery = missionsQuery.Where(m => m.TargetGroup == query.TargetGroup);
        }

        // Filter by mission type
        if (query.MissionType.HasValue)
        {
            missionsQuery = missionsQuery.Where(m => m.MissionType == query.MissionType.Value);
        }

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            missionsQuery = missionsQuery.Where(m =>
                m.Title.Contains(query.SearchTerm) ||
                (m.Description != null && m.Description.Contains(query.SearchTerm)));
        }

        // Get total count
        int totalCount = await missionsQuery.CountAsync(cancellationToken);

        // Apply pagination
        var missions = await missionsQuery
            .OrderByDescending(m => m.CreatedAt)
            .ThenBy(m => m.Title)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(m => new MissionDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Scope = m.Scope.ToString(),
                TargetClassId = m.TargetClassId,
                TargetClassCode = m.TargetClass != null ? m.TargetClass.Code : null,
                TargetClassTitle = m.TargetClass != null ? m.TargetClass.Title : null,
                TargetGroup = m.TargetGroup,
                MissionType = m.MissionType.ToString(),
                StartAt = m.StartAt,
                EndAt = m.EndAt,
                RewardStars = m.RewardStars,
                RewardExp = m.RewardExp,
                CreatedBy = m.CreatedBy,
                CreatedByName = m.CreatedByUser != null ? m.CreatedByUser.Name : null,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<MissionDto>(
            missions,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetMissionsResponse
        {
            Missions = page
        };
    }
}

