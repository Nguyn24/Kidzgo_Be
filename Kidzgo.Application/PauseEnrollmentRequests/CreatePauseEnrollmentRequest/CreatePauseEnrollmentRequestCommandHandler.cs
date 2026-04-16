using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PauseEnrollmentRequests;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.CreatePauseEnrollmentRequest;

public sealed class CreatePauseEnrollmentRequestCommandHandler(
    IDbContext context,
    PauseEnrollmentEligibleClassResolver eligibleClassResolver)
    : ICommandHandler<CreatePauseEnrollmentRequestCommand, CreatePauseEnrollmentRequestResponse>
{
    public async Task<Result<CreatePauseEnrollmentRequestResponse>> Handle(
        CreatePauseEnrollmentRequestCommand command,
        CancellationToken cancellationToken)
    {
        var profileExists = await context.Profiles
            .AnyAsync(p => p.Id == command.StudentProfileId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (!profileExists)
        {
            return Result.Failure<CreatePauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.StudentNotFound(command.StudentProfileId));
        }

        var activeEnrollments = await context.ClassEnrollments
            .Where(e => e.StudentProfileId == command.StudentProfileId
                        && e.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);

        if (activeEnrollments.Count == 0)
        {
            return Result.Failure<CreatePauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.NoEnrollmentsInRange);
        }

        var classIdsInRange = await eligibleClassResolver.GetEligibleClassIdsAsync(
            command.StudentProfileId,
            command.PauseFrom,
            command.PauseTo,
            cancellationToken);

        if (classIdsInRange.Count == 0)
        {
            return Result.Failure<CreatePauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.NoEnrollmentsInRange);
        }

        bool hasActiveRequest = await context.PauseEnrollmentRequests
            .AnyAsync(r => r.StudentProfileId == command.StudentProfileId
                           && (r.Status == PauseEnrollmentRequestStatus.Pending ||
                               r.Status == PauseEnrollmentRequestStatus.Approved)
                           && r.PauseFrom <= command.PauseTo
                           && r.PauseTo >= command.PauseFrom, cancellationToken);

        if (hasActiveRequest)
        {
            return Result.Failure<CreatePauseEnrollmentRequestResponse>(
                PauseEnrollmentRequestErrors.DuplicateActiveRequest);
        }

        var classesInRange = await context.Classes
            .Where(c => classIdsInRange.Contains(c.Id))
            .Select(c => new PauseEnrollmentClassDto
            {
                Id = c.Id,
                Code = c.Code,
                Title = c.Title,
                ProgramId = c.ProgramId,
                ProgramName = c.Program.Name,
                BranchId = c.BranchId,
                BranchName = c.Branch.Name,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var request = new PauseEnrollmentRequest
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            ClassId = null,
            PauseFrom = command.PauseFrom,
            PauseTo = command.PauseTo,
            Reason = command.Reason,
            Status = PauseEnrollmentRequestStatus.Pending,
            RequestedAt = VietnamTime.UtcNow()
        };

        context.PauseEnrollmentRequests.Add(request);
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePauseEnrollmentRequestResponse
        {
            Id = request.Id,
            StudentProfileId = request.StudentProfileId,
            PauseFrom = request.PauseFrom,
            PauseTo = request.PauseTo,
            Reason = request.Reason,
            Status = request.Status.ToString(),
            RequestedAt = request.RequestedAt,
            Classes = classesInRange
        };
    }
}
