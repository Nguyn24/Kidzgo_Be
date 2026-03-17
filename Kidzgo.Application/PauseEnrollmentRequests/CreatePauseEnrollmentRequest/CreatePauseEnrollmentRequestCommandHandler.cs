using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.CreatePauseEnrollmentRequest;

public sealed class CreatePauseEnrollmentRequestCommandHandler(IDbContext context)
    : ICommandHandler<CreatePauseEnrollmentRequestCommand, PauseEnrollmentRequestResponse>
{
    public async Task<Result<PauseEnrollmentRequestResponse>> Handle(
        CreatePauseEnrollmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var profileExists = await context.Profiles
            .AnyAsync(p => p.Id == command.StudentProfileId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (!profileExists)
        {
            return Result.Failure<PauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.StudentNotFound(command.StudentProfileId));
        }

        var classExists = await context.Classes
            .AnyAsync(c => c.Id == command.ClassId, cancellationToken);

        if (!classExists)
        {
            return Result.Failure<PauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.ClassNotFound(command.ClassId));
        }

        var enrollment = await context.ClassEnrollments
            .FirstOrDefaultAsync(e => e.ClassId == command.ClassId && e.StudentProfileId == command.StudentProfileId, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<PauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.NotEnrolled(command.ClassId, command.StudentProfileId));
        }

        if (enrollment.Status != EnrollmentStatus.Active)
        {
            return Result.Failure<PauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.EnrollmentNotActive);
        }

        bool hasActiveRequest = await context.PauseEnrollmentRequests
            .AnyAsync(r => r.StudentProfileId == command.StudentProfileId
                           && r.ClassId == command.ClassId
                           && (r.Status == PauseEnrollmentRequestStatus.Pending ||
                               r.Status == PauseEnrollmentRequestStatus.Approved), cancellationToken);

        if (hasActiveRequest)
        {
            return Result.Failure<PauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.DuplicateActiveRequest);
        }

        var request = new PauseEnrollmentRequest
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            ClassId = command.ClassId,
            PauseFrom = command.PauseFrom,
            PauseTo = command.PauseTo,
            Reason = command.Reason,
            Status = PauseEnrollmentRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        context.PauseEnrollmentRequests.Add(request);
        await context.SaveChangesAsync(cancellationToken);

        return new PauseEnrollmentRequestResponse
        {
            Id = request.Id,
            StudentProfileId = request.StudentProfileId,
            ClassId = request.ClassId,
            PauseFrom = request.PauseFrom,
            PauseTo = request.PauseTo,
            Reason = request.Reason,
            Status = request.Status.ToString(),
            RequestedAt = request.RequestedAt,
            ApprovedBy = request.ApprovedBy,
            ApprovedAt = request.ApprovedAt,
            CancelledBy = request.CancelledBy,
            CancelledAt = request.CancelledAt,
            Outcome = request.Outcome?.ToString(),
            OutcomeNote = request.OutcomeNote,
            OutcomeBy = request.OutcomeBy,
            OutcomeAt = request.OutcomeAt
        };
    }
}
