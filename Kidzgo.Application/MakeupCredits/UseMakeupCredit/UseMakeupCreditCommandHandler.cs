using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.UseMakeupCredit;

public sealed class UseMakeupCreditCommandHandler(IDbContext context, IUserContext userContext)
    : ICommandHandler<UseMakeupCreditCommand>
{
    public async Task<Result> Handle(UseMakeupCreditCommand command, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(nowUtc);

        var studentProfileId = await ResolveStudentProfileIdAsync(command, cancellationToken);
        if (studentProfileId.IsFailure)
        {
            return Result.Failure(studentProfileId.Error);
        }

        var credit = await context.MakeupCredits
            .FirstOrDefaultAsync(c => c.Id == command.MakeupCreditId, cancellationToken);

        if (credit is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(command.MakeupCreditId));
        }

        if (credit.StudentProfileId != studentProfileId.Value)
        {
            return Result.Failure(MakeupCreditErrors.NotBelongToStudent);
        }

        if (credit.ExpiresAt.HasValue && credit.ExpiresAt.Value <= nowUtc)
        {
            return Result.Failure(MakeupCreditErrors.Expired(command.MakeupCreditId));
        }

        Session? currentAllocatedSession = null;
        var canRescheduleUsedCredit = false;

        if (credit.UsedSessionId.HasValue)
        {
            currentAllocatedSession = await context.Sessions
                .AsNoTracking()
                .Include(s => s.Class)
                .ThenInclude(c => c.Program)
                .FirstOrDefaultAsync(s => s.Id == credit.UsedSessionId.Value, cancellationToken);

            if (currentAllocatedSession != null)
            {
                var allocatedDate = DateOnly.FromDateTime(currentAllocatedSession.PlannedDatetime);
                if (allocatedDate <= today)
                {
                    return Result.Failure(MakeupCreditErrors.CannotChangeAllocatedPastSession);
                }

                canRescheduleUsedCredit = true;
            }
        }

        if (credit.Status == MakeupCreditStatus.Expired)
        {
            return Result.Failure(MakeupCreditErrors.Expired(command.MakeupCreditId));
        }

        var isNewAllocation = credit.Status == MakeupCreditStatus.Available;
        var isFutureReschedule = credit.Status == MakeupCreditStatus.Used && canRescheduleUsedCredit;

        if (!isNewAllocation && !isFutureReschedule)
        {
            return Result.Failure(MakeupCreditErrors.NotAvailable(command.MakeupCreditId));
        }

        var sourceSession = await context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == credit.SourceSessionId, cancellationToken);

        if (sourceSession is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(credit.SourceSessionId));
        }

        var targetSession = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .ThenInclude(c => c.Program)
            .FirstOrDefaultAsync(s => s.Id == command.TargetSessionId, cancellationToken);

        if (targetSession is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(command.TargetSessionId));
        }

        var sourceDate = DateOnly.FromDateTime(sourceSession.PlannedDatetime);
        var targetDate = DateOnly.FromDateTime(targetSession.PlannedDatetime);

        if (targetDate < today)
        {
            return Result.Failure(MakeupCreditErrors.CannotUsePastDate);
        }

        if (targetDate.DayOfWeek != DayOfWeek.Saturday &&
            targetDate.DayOfWeek != DayOfWeek.Sunday)
        {
            return Result.Failure(MakeupCreditErrors.MustBeWeekend);
        }

        var sourceSaturday = GetSaturdayOfWeek(sourceDate);
        var targetSaturday = GetSaturdayOfWeek(targetDate);

        if (targetSaturday <= sourceSaturday)
        {
            return Result.Failure(MakeupCreditErrors.MustBeFutureWeek);
        }

        if (targetSession.ClassId != command.ClassId)
        {
            return Result.Failure(MakeupCreditErrors.SessionNotBelongToClass);
        }

        if (isFutureReschedule &&
            currentAllocatedSession is not null &&
            targetSession.Class.ProgramId != currentAllocatedSession.Class.ProgramId)
        {
            return Result.Failure(MakeupCreditErrors.MustStayInCurrentMakeupProgram);
        }

        if (isFutureReschedule &&
            currentAllocatedSession is not null &&
            currentAllocatedSession.Id == targetSession.Id)
        {
            return Result.Success();
        }

        var activeEnrollmentCount = await context.ClassEnrollments
            .CountAsync(
                e => e.ClassId == targetSession.ClassId &&
                     e.Status == EnrollmentStatus.Active,
                cancellationToken);

        var activeAllocationCount = await context.MakeupAllocations
            .CountAsync(
                a => a.TargetSessionId == targetSession.Id &&
                     a.MakeupCreditId != credit.Id &&
                     a.Status != MakeupAllocationStatus.Cancelled,
                cancellationToken);

        if (activeEnrollmentCount + activeAllocationCount >= targetSession.Class.Capacity)
        {
            return Result.Failure(MakeupCreditErrors.TargetSessionFull);
        }

        var existingAllocations = await context.MakeupAllocations
            .Where(a => a.MakeupCreditId == credit.Id && a.Status != MakeupAllocationStatus.Cancelled)
            .ToListAsync(cancellationToken);

        foreach (var existingAllocation in existingAllocations)
        {
            existingAllocation.Status = MakeupAllocationStatus.Cancelled;
        }

        credit.Status = MakeupCreditStatus.Used;
        credit.UsedSessionId = command.TargetSessionId;

        context.MakeupAllocations.Add(new MakeupAllocation
        {
            Id = Guid.NewGuid(),
            MakeupCreditId = credit.Id,
            TargetSessionId = command.TargetSessionId,
            Status = MakeupAllocationStatus.Pending,
            AssignedBy = userContext.UserId,
            AssignedAt = nowUtc,
            CreatedAt = nowUtc
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<Guid>> ResolveStudentProfileIdAsync(
        UseMakeupCreditCommand command,
        CancellationToken cancellationToken)
    {
        if (userContext.ParentId.HasValue)
        {
            if (!command.StudentProfileId.HasValue)
            {
                return Result.Failure<Guid>(MakeupCreditErrors.ParentMustProvideStudentProfileId);
            }

            var parentLink = await context.ParentStudentLinks
                .AnyAsync(
                    l => l.ParentProfileId == userContext.ParentId &&
                         l.StudentProfileId == command.StudentProfileId,
                    cancellationToken);

            if (!parentLink)
            {
                return Result.Failure<Guid>(MakeupCreditErrors.StudentNotBelongToParent);
            }

            return command.StudentProfileId.Value;
        }

        if (userContext.StudentId.HasValue)
        {
            return userContext.StudentId.Value;
        }

        return Result.Failure<Guid>(ProfileErrors.UserMustBeParentOrStudent);
    }

    private static DateOnly GetSaturdayOfWeek(DateOnly date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        return date.AddDays(DayOfWeek.Saturday - (DayOfWeek)dayOfWeek);
    }
}
