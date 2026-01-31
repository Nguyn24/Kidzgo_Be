using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Gamification.AddStars;
using Kidzgo.Application.Gamification.AddXp;
using Kidzgo.Application.Gamification.DeductStars;
using Kidzgo.Application.Gamification.DeductXp;
using Kidzgo.Application.Gamification.CheckInAttendanceStreak;
using Kidzgo.Application.Gamification.CreateRewardStoreItem;
using Kidzgo.Application.Gamification.DeleteRewardStoreItem;
using Kidzgo.Application.Gamification.GetAttendanceStreak;
using Kidzgo.Application.Gamification.GetMyAttendanceStreak;
using Kidzgo.Application.Gamification.GetRewardStoreItemById;
using Kidzgo.Application.Gamification.GetRewardStoreItems;
using Kidzgo.Application.Gamification.UpdateRewardStoreItem;
using Kidzgo.Application.Gamification.ToggleRewardStoreItemStatus;
using Kidzgo.Application.Gamification.GetMyLevel;
using Kidzgo.Application.Gamification.GetMyStarBalance;
using Kidzgo.Application.Gamification.GetStarBalance;
using Kidzgo.Application.Gamification.GetStarTransactions;
using Kidzgo.Application.Gamification.GetStudentLevel;
using Kidzgo.Application.Gamification.RequestRewardRedemption;
using Kidzgo.Application.Gamification.GetRewardRedemptions;
using Kidzgo.Application.Gamification.GetRewardRedemptionById;
using Kidzgo.Application.Gamification.GetMyRewardRedemptions;
using Kidzgo.Application.Gamification.ApproveRewardRedemption;
using Kidzgo.Application.Gamification.CancelRewardRedemption;
using Kidzgo.Application.Gamification.MarkDeliveredRewardRedemption;
using Kidzgo.Application.Gamification.ConfirmReceivedRewardRedemption;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

/// <summary>
/// UC-200 to UC-212: Stars & XP Management APIs
/// </summary>
[Route("api/gamification")]
[ApiController]
[Authorize]
public class GamificationController : ControllerBase
{
    private readonly ISender _mediator;

    public GamificationController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-205: Teacher/Staff cộng Stars thủ công
    /// </summary>
    [HttpPost("stars/add")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> AddStars(
        [FromBody] AddStarsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddStarsCommand
        {
            StudentProfileId = request.StudentProfileId,
            Amount = request.Amount,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-206: Teacher/Staff trừ Stars thủ công
    /// </summary>
    [HttpPost("stars/deduct")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> DeductStars(
        [FromBody] DeductStarsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new DeductStarsCommand
        {
            StudentProfileId = request.StudentProfileId,
            Amount = request.Amount,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-207: Teacher/Staff cộng XP thủ công
    /// </summary>
    [HttpPost("xp/add")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> AddXp(
        [FromBody] AddXpRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddXpCommand
        {
            StudentProfileId = request.StudentProfileId,
            Amount = request.Amount,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-208: Teacher/Staff trừ XP thủ công
    /// </summary>
    [HttpPost("xp/deduct")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> DeductXp(
        [FromBody] DeductXpRequest request,
        CancellationToken cancellationToken)
    {
        var command = new DeductXpCommand
        {
            StudentProfileId = request.StudentProfileId,
            Amount = request.Amount,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-209: Xem lịch sử Star Transactions
    /// </summary>
    [HttpGet("stars/transactions")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetStarTransactions(
        [FromQuery] Guid studentProfileId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStarTransactionsQuery
        {
            StudentProfileId = studentProfileId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-210: Xem balance Stars hiện tại
    /// </summary>
    [HttpGet("stars/balance")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetStarBalance(
        [FromQuery] Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var query = new GetStarBalanceQuery
        {
            StudentProfileId = studentProfileId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-212: Xem Level và XP hiện tại
    /// </summary>
    [HttpGet("level")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetStudentLevel(
        [FromQuery] Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentLevelQuery
        {
            StudentProfileId = studentProfileId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Student xem balance Stars của chính mình
    /// </summary>
    [HttpGet("stars/balance/me")]
    public async Task<IResult> GetMyStarBalance(
        CancellationToken cancellationToken)
    {
        var query = new GetMyStarBalanceQuery();

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Student xem Level và XP của chính mình
    /// </summary>
    [HttpGet("level/me")]
    public async Task<IResult> GetMyLevel(
        CancellationToken cancellationToken)
    {
        var query = new GetMyLevelQuery();

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-219: Xem Attendance Streak của học sinh (Admin/Staff/Teacher)
    /// </summary>
    [HttpGet("attendance-streak")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetAttendanceStreak(
        [FromQuery] Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var query = new GetAttendanceStreakQuery
        {
            StudentProfileId = studentProfileId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-213: Student tự điểm danh hàng ngày (Daily Check-in)
    /// </summary>
    [HttpPost("attendance-streak/check-in")]
    public async Task<IResult> CheckInAttendanceStreak(
        CancellationToken cancellationToken)
    {
        var command = new CheckInAttendanceStreakCommand();

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Student xem Attendance Streak của chính mình
    /// </summary>
    [HttpGet("attendance-streak/me")]
    public async Task<IResult> GetMyAttendanceStreak(
        CancellationToken cancellationToken)
    {
        var query = new GetMyAttendanceStreakQuery();

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    // ========== Reward Store Management (UC-221 to UC-227) ==========

    /// <summary>
    /// UC-221: Tạo Reward Store Item
    /// </summary>
    [HttpPost("reward-store/items")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateRewardStoreItem(
        [FromBody] CreateRewardStoreItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRewardStoreItemCommand
        {
            Title = request.Title,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            CostStars = request.CostStars,
            Quantity = request.Quantity,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(item => $"/api/gamification/reward-store/items/{item.Id}");
    }

    /// <summary>
    /// UC-222: Xem danh sách Reward Store Items
    /// </summary>
    [HttpGet("reward-store/items")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetRewardStoreItems(
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRewardStoreItemsQuery
        {
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-223: Xem chi tiết Reward Store Item
    /// </summary>
    [HttpGet("reward-store/items/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetRewardStoreItemById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetRewardStoreItemByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Student/Parent xem danh sách Reward Store Items đang active (có thể đặt mua)
    /// </summary>
    [HttpGet("reward-store/items/active")]
    public async Task<IResult> GetActiveRewardStoreItems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRewardStoreItemsQuery
        {
            IsActive = true, // Chỉ lấy items đang active
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-224: Cập nhật Reward Store Item
    /// UC-226: Thiết lập cost_stars cho Item
    /// UC-227: Quản lý quantity của Item
    /// </summary>
    [HttpPut("reward-store/items/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateRewardStoreItem(
        Guid id,
        [FromBody] UpdateRewardStoreItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRewardStoreItemCommand
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            CostStars = request.CostStars,
            Quantity = request.Quantity,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-225: Xóa mềm Reward Store Item
    /// </summary>
    [HttpDelete("reward-store/items/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> DeleteRewardStoreItem(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteRewardStoreItemCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Toggle trạng thái IsActive của Reward Store Item
    /// </summary>
    [HttpPatch("reward-store/items/{id:guid}/toggle-status")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ToggleRewardStoreItemStatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ToggleRewardStoreItemStatusCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    // ========== Reward Redemption Management (UC-228 to UC-237) ==========

    /// <summary>
    /// UC-228: Học sinh đổi quà (Request)
    /// </summary>
    [HttpPost("reward-redemptions")]
    public async Task<IResult> RequestRewardRedemption(
        [FromBody] RequestRewardRedemptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RequestRewardRedemptionCommand
        {
            ItemId = request.ItemId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/gamification/reward-redemptions/{r.Id}");
    }

    /// <summary>
    /// UC-229: Xem danh sách Reward Redemptions (Admin/Staff)
    /// </summary>
    [HttpGet("reward-redemptions")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetRewardRedemptions(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? itemId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        Kidzgo.Domain.Gamification.RedemptionStatus? redemptionStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Kidzgo.Domain.Gamification.RedemptionStatus>(status, true, out var parsedStatus))
        {
            redemptionStatus = parsedStatus;
        }

        var query = new GetRewardRedemptionsQuery
        {
            StudentProfileId = studentProfileId,
            ItemId = itemId,
            Status = redemptionStatus,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-230: Xem chi tiết Reward Redemption
    /// </summary>
    [HttpGet("reward-redemptions/{id:guid}")]
    public async Task<IResult> GetRewardRedemptionById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetRewardRedemptionByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Student xem danh sách Reward Redemptions của chính mình
    /// </summary>
    [HttpGet("reward-redemptions/me")]
    public async Task<IResult> GetMyRewardRedemptions(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyRewardRedemptionsQuery
        {
            Status = status,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-231: Staff duyệt Reward Redemption (APPROVED)
    /// </summary>
    [HttpPatch("reward-redemptions/{id:guid}/approve")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ApproveRewardRedemption(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ApproveRewardRedemptionCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-232: Staff từ chối Reward Redemption (CANCELLED)
    /// </summary>
    [HttpPatch("reward-redemptions/{id:guid}/cancel")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CancelRewardRedemption(
        Guid id,
        [FromBody] CancelRewardRedemptionRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new CancelRewardRedemptionCommand
        {
            Id = id,
            Reason = request?.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-233: Staff trao quà (DELIVERED)
    /// </summary>
    [HttpPatch("reward-redemptions/{id:guid}/mark-delivered")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> MarkDeliveredRewardRedemption(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new MarkDeliveredRewardRedemptionCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-234: Học sinh xác nhận nhận quà (RECEIVED)
    /// </summary>
    [HttpPatch("reward-redemptions/{id:guid}/confirm-received")]
    public async Task<IResult> ConfirmReceivedRewardRedemption(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmReceivedRewardRedemptionCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

