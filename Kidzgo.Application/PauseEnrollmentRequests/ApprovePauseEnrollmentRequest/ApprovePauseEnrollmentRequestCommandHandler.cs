using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.ApprovePauseEnrollmentRequest;

public sealed class ApprovePauseEnrollmentRequestCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<ApprovePauseEnrollmentRequestCommand>
{
    public async Task<Result> Handle(ApprovePauseEnrollmentRequestCommand request, CancellationToken cancellationToken)
    {
        var pauseRequest = await context.PauseEnrollmentRequests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (pauseRequest is null)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.NotFound(request.Id));
        }

        if (pauseRequest.Status == PauseEnrollmentRequestStatus.Approved)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.AlreadyApproved);
        }

        if (pauseRequest.Status == PauseEnrollmentRequestStatus.Rejected)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.AlreadyRejected);
        }

        if (pauseRequest.Status == PauseEnrollmentRequestStatus.Cancelled)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.AlreadyCancelled);
        }

        var activeEnrollments = await context.ClassEnrollments
            .Where(e => e.StudentProfileId == pauseRequest.StudentProfileId
                        && e.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);

        var requestEnrollment = activeEnrollments
            .FirstOrDefault(e => e.ClassId == pauseRequest.ClassId);

        if (requestEnrollment is null)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.NotEnrolled(
                pauseRequest.ClassId,
                pauseRequest.StudentProfileId));
        }

        pauseRequest.Status = PauseEnrollmentRequestStatus.Approved;
        pauseRequest.ApprovedAt = DateTime.UtcNow;
        pauseRequest.ApprovedBy = userContext.UserId;

        var activeClassIds = activeEnrollments
            .Select(e => e.ClassId)
            .Distinct()
            .ToList();

        var classIdsInRange = await context.Sessions
            .Where(s => activeClassIds.Contains(s.ClassId)
                        && DateOnly.FromDateTime(s.PlannedDatetime) >= pauseRequest.PauseFrom
                        && DateOnly.FromDateTime(s.PlannedDatetime) <= pauseRequest.PauseTo)
            .Select(s => s.ClassId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!classIdsInRange.Contains(pauseRequest.ClassId))
        {
            classIdsInRange.Add(pauseRequest.ClassId);
        }

        var affectedEnrollments = activeEnrollments
            .Where(e => classIdsInRange.Contains(e.ClassId))
            .ToList();

        foreach (var enrollment in affectedEnrollments)
        {
            var previousStatus = enrollment.Status;
            enrollment.Status = EnrollmentStatus.Paused;
            enrollment.UpdatedAt = DateTime.UtcNow;

            var history = new PauseEnrollmentRequestHistory
            {
                Id = Guid.NewGuid(),
                PauseEnrollmentRequestId = pauseRequest.Id,
                StudentProfileId = enrollment.StudentProfileId,
                ClassId = enrollment.ClassId,
                EnrollmentId = enrollment.Id,
                PreviousStatus = previousStatus,
                NewStatus = EnrollmentStatus.Paused,
                PauseFrom = pauseRequest.PauseFrom,
                PauseTo = pauseRequest.PauseTo,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = userContext.UserId
            };

            context.PauseEnrollmentRequestHistories.Add(history);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
