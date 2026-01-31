using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetMyRewardRedemptions;

/// <summary>
/// Student xem danh sách Reward Redemptions của chính mình
/// </summary>
public sealed class GetMyRewardRedemptionsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetMyRewardRedemptionsQuery, GetMyRewardRedemptionsResponse>
{
    public async Task<Result<GetMyRewardRedemptionsResponse>> Handle(
        GetMyRewardRedemptionsQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.StudentId.HasValue)
        {
            return Result.Failure<GetMyRewardRedemptionsResponse>(
                RewardRedemptionErrors.NotFound(Guid.Empty)); // Generic error
        }

        var studentProfileId = userContext.StudentId.Value;

        var redemptionsQuery = context.RewardRedemptions
            .Include(r => r.HandledByUser)
            .Where(r => r.StudentProfileId == studentProfileId)
            .AsQueryable();

        // Filter by status
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<RedemptionStatus>(query.Status, true, out var status))
        {
            redemptionsQuery = redemptionsQuery.Where(r => r.Status == status);
        }

        var totalCount = await redemptionsQuery.CountAsync(cancellationToken);

        var redemptions = await redemptionsQuery
            .OrderByDescending(r => r.CreatedAt)
            .ApplyPagination(query.Page, query.PageSize)
            .Select(r => new RewardRedemptionDto
            {
                Id = r.Id,
                ItemId = r.ItemId,
                ItemName = r.ItemName,
                StudentProfileId = r.StudentProfileId,
                Status = r.Status.ToString(),
                HandledBy = r.HandledBy,
                HandledByName = r.HandledByUser != null ? (r.HandledByUser.Name ?? r.HandledByUser.Email) : null,
                HandledAt = r.HandledAt,
                DeliveredAt = r.DeliveredAt,
                ReceivedAt = r.ReceivedAt,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<RewardRedemptionDto>(redemptions, totalCount, query.Page, query.PageSize);

        return Result.Success(new GetMyRewardRedemptionsResponse
        {
            Redemptions = page
        });
    }
}

