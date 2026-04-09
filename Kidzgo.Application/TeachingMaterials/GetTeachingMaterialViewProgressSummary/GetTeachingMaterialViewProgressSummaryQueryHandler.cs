using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.TeachingMaterials.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialViewProgressSummary;

public sealed class GetTeachingMaterialViewProgressSummaryQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeachingMaterialViewProgressSummaryQuery, GetTeachingMaterialViewProgressSummaryResponse>
{
    public async Task<Result<GetTeachingMaterialViewProgressSummaryResponse>> Handle(
        GetTeachingMaterialViewProgressSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var role = await TeachingMaterialAccessHelper.GetCurrentUserRoleAsync(context, userContext, cancellationToken);
        if (role is not (UserRole.Admin or UserRole.Teacher))
        {
            return Result.Failure<GetTeachingMaterialViewProgressSummaryResponse>(
                TeachingMaterialErrors.AccessDenied(query.TeachingMaterialId));
        }

        var materialExists = await context.TeachingMaterials
            .AnyAsync(tm => tm.Id == query.TeachingMaterialId, cancellationToken);

        if (!materialExists)
        {
            return Result.Failure<GetTeachingMaterialViewProgressSummaryResponse>(
                TeachingMaterialErrors.NotFound(query.TeachingMaterialId));
        }

        var viewers = await context.TeachingMaterialViewProgresses
            .AsNoTracking()
            .Where(progress => progress.TeachingMaterialId == query.TeachingMaterialId)
            .Include(progress => progress.User)
            .OrderByDescending(progress => progress.LastViewedAt)
            .Select(progress => new TeachingMaterialProgressViewerDto
            {
                UserId = progress.UserId,
                UserName = progress.User.Name ?? progress.User.Username ?? progress.User.Email,
                AvatarUrl = progress.User.AvatarUrl,
                ProgressPercent = progress.ProgressPercent,
                LastSlideViewed = progress.LastSlideViewed,
                TotalTimeSeconds = progress.TotalTimeSeconds,
                ViewCount = progress.ViewCount,
                Completed = progress.Completed,
                LastViewedAt = progress.LastViewedAt
            })
            .ToListAsync(cancellationToken);

        return new GetTeachingMaterialViewProgressSummaryResponse
        {
            MaterialId = query.TeachingMaterialId,
            TotalViewers = viewers.Count,
            CompletedCount = viewers.Count(viewer => viewer.Completed),
            AverageProgressPercent = viewers.Count == 0
                ? 0
                : Math.Round((decimal)viewers.Average(viewer => viewer.ProgressPercent), 2),
            AverageTimeSeconds = viewers.Count == 0
                ? 0
                : Math.Round((decimal)viewers.Average(viewer => viewer.TotalTimeSeconds), 2),
            Viewers = viewers
        };
    }
}
