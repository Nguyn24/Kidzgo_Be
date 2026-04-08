using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests.Notifications;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.ApprovePauseEnrollmentRequest;

public sealed class ApprovePauseEnrollmentRequestCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ITemplateRenderer templateRenderer,
    StudentSessionAssignmentService studentSessionAssignmentService)
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

        if (activeEnrollments.Count == 0)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.NoEnrollmentsInRange);
        }

        if (pauseRequest.ClassId.HasValue)
        {
            var requestEnrollment = activeEnrollments
                .FirstOrDefault(e => e.ClassId == pauseRequest.ClassId.Value);

            if (requestEnrollment is null)
            {
                return Result.Failure(PauseEnrollmentRequestErrors.NotEnrolled(
                    pauseRequest.ClassId.Value,
                    pauseRequest.StudentProfileId));
            }
        }

        pauseRequest.Status = PauseEnrollmentRequestStatus.Approved;
        pauseRequest.ApprovedAt = VietnamTime.UtcNow();
        pauseRequest.ApprovedBy = userContext.UserId;

        var activeClassIds = activeEnrollments
            .Select(e => e.ClassId)
            .Distinct()
            .ToList();
        var pauseFromUtc = VietnamTime.TreatAsVietnamLocal(pauseRequest.PauseFrom.ToDateTime(TimeOnly.MinValue));
        var pauseToUtc = VietnamTime.EndOfVietnamDayUtc(VietnamTime.TreatAsVietnamLocal(pauseRequest.PauseTo.ToDateTime(TimeOnly.MinValue)));

        var classIdsInRange = await context.Sessions
            .Where(s => activeClassIds.Contains(s.ClassId)
                        && s.PlannedDatetime >= pauseFromUtc
                        && s.PlannedDatetime <= pauseToUtc)
            .Select(s => s.ClassId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (pauseRequest.ClassId.HasValue && !classIdsInRange.Contains(pauseRequest.ClassId.Value))
        {
            classIdsInRange.Add(pauseRequest.ClassId.Value);
        }

        if (classIdsInRange.Count == 0)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.NoEnrollmentsInRange);
        }

        var affectedEnrollments = activeEnrollments
            .Where(e => classIdsInRange.Contains(e.ClassId))
            .ToList();

        foreach (var enrollment in affectedEnrollments)
        {
            var previousStatus = enrollment.Status;
            enrollment.Status = EnrollmentStatus.Paused;
            enrollment.UpdatedAt = VietnamTime.UtcNow();
            await studentSessionAssignmentService.CancelAssignmentsForEnrollmentInRangeAsync(
                enrollment.Id,
                pauseFromUtc,
                pauseToUtc,
                cancellationToken);

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
                ChangedAt = VietnamTime.UtcNow(),
                ChangedBy = userContext.UserId
            };

            context.PauseEnrollmentRequestHistories.Add(history);
        }

        await context.SaveChangesAsync(cancellationToken);

        await PauseEnrollmentRequestNotificationHelper.NotifyAsync(
            context,
            templateRenderer,
            pauseRequest.StudentProfileId,
            pauseRequest.Id,
            PauseEnrollmentRequestNotificationHelper.NotificationType.Approved,
            pauseRequest.PauseFrom,
            pauseRequest.PauseTo,
            null,
            null,
            cancellationToken);

        return Result.Success();
    }
}
