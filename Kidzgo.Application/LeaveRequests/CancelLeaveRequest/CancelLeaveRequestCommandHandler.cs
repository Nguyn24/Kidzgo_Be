using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LeaveRequests.CancelLeaveRequest;

public sealed class CancelLeaveRequestCommandHandler(IDbContext context)
    : ICommandHandler<CancelLeaveRequestCommand>
{
    public async Task<Result> Handle(CancelLeaveRequestCommand command, CancellationToken cancellationToken)
    {
        var leaveRequest = await context.LeaveRequests
            .Include(lr => lr.Class)
            .FirstOrDefaultAsync(lr => lr.Id == command.Id, cancellationToken);

        if (leaveRequest is null)
        {
            return Result.Failure(LeaveRequestErrors.NotFound(command.Id));
        }

        // Check if already cancelled
        if (leaveRequest.Status == LeaveRequestStatus.Cancelled)
        {
            return Result.Failure(LeaveRequestErrors.AlreadyCancelled);
        }

        // Check if session date has passed - cannot cancel past sessions
        if (leaveRequest.SessionDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return Result.Failure(LeaveRequestErrors.CannotCancelPastSession(leaveRequest.SessionDate));
        }

        // Check if was approved (to know if we need to delete makeup credits)
        bool wasApproved = await WasOriginallyApproved(leaveRequest, cancellationToken);

        // Update status to Cancelled
        leaveRequest.Status = LeaveRequestStatus.Cancelled;
        leaveRequest.CancelledAt = DateTime.UtcNow;

        // If was approved, also delete the makeup credits and allocations
        if (wasApproved)
        {
            var sourceSession = leaveRequest.SessionId.HasValue
                ? await context.Sessions
                    .FirstOrDefaultAsync(s => s.Id == leaveRequest.SessionId.Value, cancellationToken)
                : await context.Sessions
                    .FirstOrDefaultAsync(s => s.ClassId == leaveRequest.ClassId
                        && DateOnly.FromDateTime(s.PlannedDatetime) == leaveRequest.SessionDate, cancellationToken);

            if (sourceSession != null)
            {
                var makeupCredits = await context.MakeupCredits
                    .Where(mc => mc.StudentProfileId == leaveRequest.StudentProfileId
                        && mc.SourceSessionId == sourceSession.Id)
                    .Include(mc => mc.MakeupAllocations)
                    .ToListAsync(cancellationToken);

                foreach (var credit in makeupCredits)
                {
                    // Remove allocations first
                    context.MakeupAllocations.RemoveRange(credit.MakeupAllocations);
                    // Then remove credits
                    context.MakeupCredits.Remove(credit);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<bool> WasOriginallyApproved(LeaveRequest leaveRequest, CancellationToken cancellationToken)
    {
        var sourceSession = leaveRequest.SessionId.HasValue
            ? await context.Sessions
                .FirstOrDefaultAsync(s => s.Id == leaveRequest.SessionId.Value, cancellationToken)
            : await context.Sessions
                .FirstOrDefaultAsync(s => s.ClassId == leaveRequest.ClassId
                    && DateOnly.FromDateTime(s.PlannedDatetime) == leaveRequest.SessionDate, cancellationToken);

        if (sourceSession == null) return false;

        return await context.MakeupCredits
            .AnyAsync(mc => mc.StudentProfileId == leaveRequest.StudentProfileId
                && mc.SourceSessionId == sourceSession.Id
                && mc.CreatedReason == CreatedReason.ApprovedLeave24H, cancellationToken);
    }
}
