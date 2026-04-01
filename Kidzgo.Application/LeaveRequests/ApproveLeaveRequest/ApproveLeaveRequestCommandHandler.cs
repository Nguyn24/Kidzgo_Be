using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LeaveRequests.ApproveLeaveRequest;

public sealed class ApproveLeaveRequestCommandHandler(IDbContext context, IUserContext userContext)
    : ICommandHandler<ApproveLeaveRequestCommand>
{
    public async Task<Result> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leave = await context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (leave is null)
        {
            return Result.Failure(LeaveRequestErrors.NotFound(request.Id));
        }

        if (leave.Status == LeaveRequestStatus.Approved)
        {
            return Result.Failure(LeaveRequestErrors.AlreadyApproved);
        }

        leave.Status = LeaveRequestStatus.Approved;
        leave.ApprovedAt = DateTime.UtcNow;
        leave.ApprovedBy = userContext.UserId;

        // Tạo MakeupCredit cho tất cả các buổi học trong khoảng ngày xin nghỉ
        var sessionsInRange = leave.SessionId.HasValue
            ? await context.Sessions
                .Where(s => s.Id == leave.SessionId.Value)
                .ToListAsync(cancellationToken)
            : await context.Sessions
                .Where(s => s.ClassId == leave.ClassId
                            && DateOnly.FromDateTime(s.PlannedDatetime) >= leave.SessionDate
                            && DateOnly.FromDateTime(s.PlannedDatetime) <= (leave.EndDate ?? leave.SessionDate))
                .ToListAsync(cancellationToken);

        if (!sessionsInRange.Any())
        {
            return Result.Failure(LeaveRequestErrors.SessionNotFound(leave.ClassId, leave.SessionDate));
        }

        foreach (var session in sessionsInRange)
        {
            bool creditExists = await context.MakeupCredits
                .AnyAsync(c => c.StudentProfileId == leave.StudentProfileId &&
                               c.CreatedReason == CreatedReason.ApprovedLeave24H &&
                               c.SourceSessionId == session.Id, cancellationToken);

            // Luôn tạo MakeupCredit khi đơn xin nghỉ được approve (không còn điều kiện > 24h),
            // nhưng tránh tạo trùng cho cùng 1 session.
            if (!creditExists)
            {
                var credit = new MakeupCredit
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = leave.StudentProfileId,
                    SourceSessionId = session.Id,
                    Status = MakeupCreditStatus.Available,
                    CreatedReason = CreatedReason.ApprovedLeave24H,
                    ExpiresAt = null,
                    CreatedAt = DateTime.UtcNow
                };
                context.MakeupCredits.Add(credit);

                // Tự động xếp lịch bù vào T7/CN của tuần student nghỉ
                await AutoScheduleMakeupForWeekendAsync(
                    context,
                    credit,
                    session,
                    DateOnly.FromDateTime(session.PlannedDatetime),
                    cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    /// <summary>
    /// Tự động xếp lịch bù vào T7/CN của tuần student nghỉ
    /// </summary>
    private async Task AutoScheduleMakeupForWeekendAsync(
        IDbContext context,
        MakeupCredit credit,
        Session sourceSession,
        DateOnly leaveDate,
        CancellationToken cancellationToken)
    {
        // Tìm thứ 7 và chủ nhật của tuần student nghỉ
        var dayOfWeek = (int)leaveDate.DayOfWeek;
        var saturday = leaveDate.AddDays(DayOfWeek.Saturday - (DayOfWeek)dayOfWeek);
        var sunday = saturday.AddDays(1);

        // Lấy thông tin class gốc để biết program level và branch
        var classInfo = await context.Classes
            .Include(c => c.Program)
            .FirstOrDefaultAsync(c => c.Id == sourceSession.ClassId, cancellationToken);

        if (classInfo == null) return;

        // Tìm các session T7/CN cùng tuần, cùng level, cùng branch, khác class
        var weekendSessions = await context.Sessions
            .Include(s => s.Class)
            .ThenInclude(c => c.Program)
            .Where(s => s.BranchId == sourceSession.BranchId)
            .Where(s => s.ClassId != sourceSession.ClassId)
            .Where(s => s.Status == SessionStatus.Scheduled)
            .Where(s => s.PlannedDatetime >= DateTime.UtcNow)
            .Where(s => DateOnly.FromDateTime(s.PlannedDatetime) == saturday ||
                        DateOnly.FromDateTime(s.PlannedDatetime) == sunday)
            .ToListAsync(cancellationToken);

        // Random shuffle để chọn ngẫu nhiên
        var random = new Random();
        var shuffledSessions = weekendSessions.OrderBy(_ => random.Next()).ToList();

        // Tìm session còn slot và tạo MakeupAllocation
        foreach (var weekendSession in shuffledSessions)
        {
            // Kiểm tra xem session còn slot không
            var participantCount = await GetScheduledParticipantCountAsync(context, weekendSession, cancellationToken);

            if (participantCount < weekendSession.Class.Capacity)
            {
                // Tạo MakeupAllocation
                var allocation = new MakeupAllocation
                {
                    Id = Guid.NewGuid(),
                    MakeupCreditId = credit.Id,
                    TargetSessionId = weekendSession.Id,
                    AssignedAt = DateTime.UtcNow
                };
                context.MakeupAllocations.Add(allocation);

                // Cập nhật trạng thái makeup credit đã được sử dụng
                credit.Status = MakeupCreditStatus.Used;
                credit.UsedSessionId = weekendSession.Id;

                break; // Chỉ assign vào 1 session đầu tiên còn slot
            }
        }
    }

    private static async Task<int> GetScheduledParticipantCountAsync(
        IDbContext context,
        Session session,
        CancellationToken cancellationToken)
    {
        var hasAssignments = await context.StudentSessionAssignments
            .AnyAsync(a => a.SessionId == session.Id, cancellationToken);

        var regularCount = hasAssignments
            ? await context.StudentSessionAssignments
                .CountAsync(a => a.SessionId == session.Id && a.Status == StudentSessionAssignmentStatus.Assigned, cancellationToken)
            : await context.ClassEnrollments
                .CountAsync(e => e.ClassId == session.ClassId
                    && e.Status == EnrollmentStatus.Active
                    && e.EnrollDate <= DateOnly.FromDateTime(session.PlannedDatetime), cancellationToken);

        var makeupCount = await context.MakeupAllocations
            .CountAsync(a => a.TargetSessionId == session.Id && a.Status != MakeupAllocationStatus.Cancelled, cancellationToken);

        return regularCount + makeupCount;
    }
}

