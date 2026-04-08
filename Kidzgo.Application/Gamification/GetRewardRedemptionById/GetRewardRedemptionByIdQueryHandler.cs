using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Gamification.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.GetRewardRedemptionById;

/// <summary>
/// UC-230: Xem chi tiết Reward Redemption
/// </summary>
public sealed class GetRewardRedemptionByIdQueryHandler(IDbContext context)
    : IQueryHandler<GetRewardRedemptionByIdQuery, GetRewardRedemptionByIdResponse>
{
    public async Task<Result<GetRewardRedemptionByIdResponse>> Handle(
        GetRewardRedemptionByIdQuery query,
        CancellationToken cancellationToken)
    {
        var redemption = await context.RewardRedemptions
            .Include(r => r.StudentProfile)
                .ThenInclude(p => p.User)
            .Include(r => r.StudentProfile)
                .ThenInclude(p => p.ClassEnrollments)
                    .ThenInclude(e => e.Class)
                        .ThenInclude(c => c.Branch)
            .Include(r => r.HandledByUser)
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

        if (redemption == null)
        {
            return Result.Failure<GetRewardRedemptionByIdResponse>(
                RewardRedemptionErrors.NotFound(query.Id));
        }

        // Lấy branch name từ enrollment active gần nhất
        var activeEnrollment = redemption.StudentProfile?.ClassEnrollments
            .Where(e => e.Status == EnrollmentStatus.Active)
            .OrderByDescending(e => e.EnrollDate)
            .FirstOrDefault();

        var branchName = activeEnrollment?.Class?.Branch?.Name;

        return Result.Success(new GetRewardRedemptionByIdResponse
        {
            Id = redemption.Id,
            ItemId = redemption.ItemId,
            ItemName = redemption.ItemName,
            Quantity = redemption.Quantity,
            StarsDeducted = redemption.StarsDeducted,
            StudentProfileId = redemption.StudentProfileId,
            StudentName = redemption.StudentProfile != null ? (redemption.StudentProfile.DisplayName ?? (redemption.StudentProfile.User != null ? (redemption.StudentProfile.User.Name ?? redemption.StudentProfile.User.Email) : null)) : null,
            BranchName = branchName,
            Status = redemption.Status.ToString(),
            CancelReason = redemption.CancelReason,
            HandledBy = redemption.HandledBy,
            HandledByName = redemption.HandledByUser != null ? (redemption.HandledByUser.Name ?? redemption.HandledByUser.Email) : null,
            HandledAt = redemption.HandledAt,
            DeliveredAt = redemption.DeliveredAt,
            ReceivedAt = redemption.ReceivedAt,
            CreatedAt = redemption.CreatedAt
        });
    }
}

