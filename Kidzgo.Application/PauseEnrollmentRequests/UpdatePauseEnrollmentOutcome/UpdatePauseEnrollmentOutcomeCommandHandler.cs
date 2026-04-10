using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes;
using Kidzgo.Application.PauseEnrollmentRequests.Notifications;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.UpdatePauseEnrollmentOutcome;

public sealed class UpdatePauseEnrollmentOutcomeCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ITemplateRenderer templateRenderer,
    StudentSessionAssignmentService studentSessionAssignmentService)
    : ICommandHandler<UpdatePauseEnrollmentOutcomeCommand>
{
    public async Task<Result> Handle(UpdatePauseEnrollmentOutcomeCommand request, CancellationToken cancellationToken)
    {
        var pauseRequest = await context.PauseEnrollmentRequests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (pauseRequest is null)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.NotFound(request.Id));
        }

        if (pauseRequest.Status != PauseEnrollmentRequestStatus.Approved)
        {
            return Result.Failure(PauseEnrollmentRequestErrors.OutcomeNotAllowed);
        }

        if (request.Outcome == PauseEnrollmentOutcome.ContinueSameClass)
        {
            var reactivateResult = await ReactivatePausedEnrollmentsAsync(pauseRequest, cancellationToken);
            if (reactivateResult.IsFailure)
            {
                return reactivateResult;
            }
        }

        pauseRequest.Outcome = request.Outcome;
        pauseRequest.OutcomeNote = request.OutcomeNote;
        pauseRequest.OutcomeBy = userContext.UserId;
        pauseRequest.OutcomeAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        await PauseEnrollmentRequestNotificationHelper.NotifyAsync(
            context,
            templateRenderer,
            pauseRequest.StudentProfileId,
            pauseRequest.Id,
            PauseEnrollmentRequestNotificationHelper.NotificationType.OutcomeUpdated,
            pauseRequest.PauseFrom,
            pauseRequest.PauseTo,
            request.Outcome.ToString(),
            request.OutcomeNote,
            cancellationToken);

        return Result.Success();
    }

    private async Task<Result> ReactivatePausedEnrollmentsAsync(
        PauseEnrollmentRequest pauseRequest,
        CancellationToken cancellationToken)
    {
        var enrollmentIds = await context.PauseEnrollmentRequestHistories
            .Where(h => h.PauseEnrollmentRequestId == pauseRequest.Id &&
                        h.EnrollmentId.HasValue &&
                        h.NewStatus == EnrollmentStatus.Paused)
            .Select(h => h.EnrollmentId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (enrollmentIds.Count == 0)
        {
            return Result.Success();
        }

        var enrollments = await context.ClassEnrollments
            .Include(e => e.Class)
            .Where(e => enrollmentIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        if (enrollments.Any(e => e.Status == EnrollmentStatus.Dropped))
        {
            return Result.Failure(EnrollmentErrors.CannotReactivateDropped);
        }

        var now = VietnamTime.UtcNow();
        var today = VietnamTime.ToVietnamDateOnly(now);
        var effectiveFrom = pauseRequest.PauseTo.AddDays(1);
        if (today > effectiveFrom)
        {
            effectiveFrom = today;
        }

        var pausedEnrollments = enrollments
            .Where(e => e.Status == EnrollmentStatus.Paused)
            .ToList();

        var reactivationCountsByClass = pausedEnrollments
            .GroupBy(e => e.ClassId)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var classId in reactivationCountsByClass.Keys)
        {
            var classEntity = enrollments
                .First(e => e.ClassId == classId)
                .Class;

            var currentEnrollmentCount = await context.ClassEnrollments
                .CountAsync(ce => ce.ClassId == classId && ce.Status == EnrollmentStatus.Active, cancellationToken);

            ClassCapacityStatusHelper.SyncAvailabilityStatus(classEntity, currentEnrollmentCount, now);

            if (classEntity.Status != ClassStatus.Active &&
                classEntity.Status != ClassStatus.Planned &&
                classEntity.Status != ClassStatus.Recruiting)
            {
                return Result.Failure(EnrollmentErrors.ClassNotAvailable);
            }

            if (currentEnrollmentCount + reactivationCountsByClass[classId] > classEntity.Capacity)
            {
                return Result.Failure(EnrollmentErrors.ClassFull);
            }
        }

        foreach (var enrollment in pausedEnrollments)
        {
            enrollment.Status = EnrollmentStatus.Active;
            enrollment.UpdatedAt = now;
            await studentSessionAssignmentService.RestoreAssignmentsForEnrollmentAsync(
                enrollment,
                effectiveFrom,
                cancellationToken);

            context.PauseEnrollmentRequestHistories.Add(new PauseEnrollmentRequestHistory
            {
                Id = Guid.NewGuid(),
                PauseEnrollmentRequestId = pauseRequest.Id,
                StudentProfileId = enrollment.StudentProfileId,
                ClassId = enrollment.ClassId,
                EnrollmentId = enrollment.Id,
                PreviousStatus = EnrollmentStatus.Paused,
                NewStatus = EnrollmentStatus.Active,
                PauseFrom = pauseRequest.PauseFrom,
                PauseTo = pauseRequest.PauseTo,
                ChangedAt = now,
                ChangedBy = userContext.UserId
            });
        }

        foreach (var classId in reactivationCountsByClass.Keys)
        {
            var classEntity = enrollments
                .First(e => e.ClassId == classId)
                .Class;
            var currentEnrollmentCount = await context.ClassEnrollments
                .CountAsync(ce => ce.ClassId == classId && ce.Status == EnrollmentStatus.Active, cancellationToken);
            ClassCapacityStatusHelper.SyncAvailabilityStatus(
                classEntity,
                currentEnrollmentCount + reactivationCountsByClass[classId],
                now);
        }

        return Result.Success();
    }
}
