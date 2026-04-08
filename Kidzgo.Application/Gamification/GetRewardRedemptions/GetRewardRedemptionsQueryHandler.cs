using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetRewardRedemptions;

/// <summary>
/// UC-229: Xem danh sách Reward Redemptions
/// </summary>
public sealed class GetRewardRedemptionsQueryHandler(IDbContext context)
    : IQueryHandler<GetRewardRedemptionsQuery, GetRewardRedemptionsResponse>
{
    public async Task<Result<GetRewardRedemptionsResponse>> Handle(
        GetRewardRedemptionsQuery query,
        CancellationToken cancellationToken)
    {
        var redemptionsQuery = context.RewardRedemptions
            .Include(r => r.StudentProfile)
                .ThenInclude(p => p.User)
            .Include(r => r.StudentProfile)
                .ThenInclude(p => p.ClassEnrollments)
                    .ThenInclude(e => e.Class)
                        .ThenInclude(c => c.Branch)
            .Include(r => r.HandledByUser)
            .AsQueryable();

        // Filter by student profile
        if (query.StudentProfileId.HasValue)
        {
            redemptionsQuery = redemptionsQuery.Where(r => r.StudentProfileId == query.StudentProfileId.Value);
        }

        // Filter by item
        if (query.ItemId.HasValue)
        {
            redemptionsQuery = redemptionsQuery.Where(r => r.ItemId == query.ItemId.Value);
        }

        // Filter by status
        if (query.Status.HasValue)
        {
            redemptionsQuery = redemptionsQuery.Where(r => r.Status == query.Status.Value);
        }

        var totalCount = await redemptionsQuery.CountAsync(cancellationToken);

        var redemptions = await redemptionsQuery
            .OrderByDescending(r => r.CreatedAt)
            .ApplyPagination(query.Page, query.PageSize)
            .ToListAsync(cancellationToken);

        var redemptionDtos = redemptions.Select(r =>
        {
            // Lấy branch name từ enrollment active gần nhất
            var activeEnrollment = r.StudentProfile?.ClassEnrollments
                .Where(e => e.Status == EnrollmentStatus.Active)
                .OrderByDescending(e => e.EnrollDate)
                .FirstOrDefault();

            var branchName = activeEnrollment?.Class?.Branch?.Name;

            return new RewardRedemptionDto
            {
                Id = r.Id,
                ItemId = r.ItemId,
                ItemName = r.ItemName,
                Quantity = r.Quantity,
                StarsDeducted = r.StarsDeducted,
                StudentProfileId = r.StudentProfileId,
                StudentName = r.StudentProfile != null ? (r.StudentProfile.DisplayName ?? (r.StudentProfile.User != null ? (r.StudentProfile.User.Name ?? r.StudentProfile.User.Email) : null)) : null,
                BranchName = branchName,
                Status = r.Status.ToString(),
                CancelReason = r.CancelReason,
                HandledBy = r.HandledBy,
                HandledByName = r.HandledByUser != null ? (r.HandledByUser.Name ?? r.HandledByUser.Email) : null,
                HandledAt = r.HandledAt,
                DeliveredAt = r.DeliveredAt,
                ReceivedAt = r.ReceivedAt,
                CreatedAt = r.CreatedAt
            };
        }).ToList();

        var page = new Page<RewardRedemptionDto>(redemptionDtos, totalCount, query.Page, query.PageSize);

        return Result.Success(new GetRewardRedemptionsResponse
        {
            Redemptions = page
        });
    }
}

