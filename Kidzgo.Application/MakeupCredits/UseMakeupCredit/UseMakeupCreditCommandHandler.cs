using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
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
        // Xác định StudentProfileId:
        // - Nếu là parent: sử dụng StudentProfileId từ request (phải thuộc parent đó)
        // - Nếu là student: sử dụng StudentId từ userContext
        Guid studentProfileId;

        if (userContext.ParentId.HasValue)
        {
            // Parent: bắt buộc phải truyền StudentProfileId
            if (!command.StudentProfileId.HasValue)
            {
                return Result.Failure(MakeupCreditErrors.ParentMustProvideStudentProfileId);
            }

            // Kiểm tra student thuộc parent này
            var parentLink = await context.ParentStudentLinks
                .AnyAsync(l => l.ParentProfileId == userContext.ParentId &&
                               l.StudentProfileId == command.StudentProfileId, cancellationToken);

            if (!parentLink)
            {
                return Result.Failure(MakeupCreditErrors.StudentNotBelongToParent);
            }

            studentProfileId = command.StudentProfileId.Value;
        }
        else if (userContext.StudentId.HasValue)
        {
            // Student: sử dụng từ context
            studentProfileId = userContext.StudentId.Value;
        }
        else
        {
            return Result.Failure(ProfileErrors.UserMustBeParentOrStudent);
        }

        var credit = await context.MakeupCredits
            .FirstOrDefaultAsync(c => c.Id == command.MakeupCreditId, cancellationToken);

        if (credit is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(command.MakeupCreditId));
        }

        // Kiểm tra credit thuộc về student đang đăng nhập
        if (credit.StudentProfileId != studentProfileId)
        {
            return Result.Failure(MakeupCreditErrors.NotBelongToStudent);
        }

        // Kiểm tra nếu credit đã được xếp lịch (đã có UsedSessionId)
        // thì kiểm tra xem ngày học bù đã qua chưa - nếu đã qua thì không cho đổi
        if (credit.UsedSessionId.HasValue)
        {
            var currentAllocatedSession = await context.Sessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == credit.UsedSessionId, cancellationToken);

            if (currentAllocatedSession != null)
            {
                var allocatedDate = DateOnly.FromDateTime(currentAllocatedSession.PlannedDatetime);
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                if (allocatedDate <= today)
                {
                    return Result.Failure(MakeupCreditErrors.CannotChangeAllocatedPastSession);
                }
            }
        }

        if (credit.Status != MakeupCreditStatus.Available)
        {
            return Result.Failure(MakeupCreditErrors.NotAvailable(command.MakeupCreditId));
        }

        if (credit.ExpiresAt.HasValue && credit.ExpiresAt.Value <= DateTime.UtcNow)
        {
            return Result.Failure(MakeupCreditErrors.Expired(command.MakeupCreditId));
        }

        // Lấy thông tin source session để biết tuần nghỉ
        var sourceSession = await context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == credit.SourceSessionId, cancellationToken);

        if (sourceSession is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(credit.SourceSessionId));
        }

        var sourceDate = DateOnly.FromDateTime(sourceSession.PlannedDatetime);
        var now = DateOnly.FromDateTime(DateTime.UtcNow);

        // Kiểm tra target session
        var targetSession = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .ThenInclude(c => c.Program)
            .FirstOrDefaultAsync(s => s.Id == command.TargetSessionId, cancellationToken);

        if (targetSession is null)
        {
            return Result.Failure(MakeupCreditErrors.NotFound(command.TargetSessionId));
        }

        var targetDate = DateOnly.FromDateTime(targetSession.PlannedDatetime);
        var targetDayOfWeek = targetDate.DayOfWeek;

        // Validation 1: Target session không được là ngày trong quá khứ
        if (targetDate < now)
        {
            return Result.Failure(MakeupCreditErrors.CannotUsePastDate);
        }

        // Validation 2: Target session phải là T7 hoặc CN
        if (targetDayOfWeek != DayOfWeek.Saturday && targetDayOfWeek != DayOfWeek.Sunday)
        {
            return Result.Failure(MakeupCreditErrors.MustBeWeekend);
        }

        // Validation 3: Target session phải thuộc các tuần SAU tuần nghỉ
        // Tìm thứ 7 của tuần source session
        var sourceSaturday = GetSaturdayOfWeek(sourceDate);
        var targetSaturday = GetSaturdayOfWeek(targetDate);

        // Target phải là tuần sau tuần source (không phải cùng tuần)
        if (targetSaturday <= sourceSaturday)
        {
            return Result.Failure(MakeupCreditErrors.MustBeFutureWeek);
        }

        if (targetSession.ClassId != command.ClassId)
        {
            return Result.Failure(MakeupCreditErrors.SessionNotBelongToClass);
        }

        credit.Status = MakeupCreditStatus.Used;
        credit.UsedSessionId = command.TargetSessionId;

        // Lấy profileId của người assign (student hoặc parent)
        Guid? assignedByProfileId = userContext.StudentId ?? userContext.ParentId;

        var allocation = new MakeupAllocation
        {
            Id = Guid.NewGuid(),
            MakeupCreditId = credit.Id,
            TargetSessionId = command.TargetSessionId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedByProfileId
        };

        context.MakeupAllocations.Add(allocation);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static DateOnly GetSaturdayOfWeek(DateOnly date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        return date.AddDays(DayOfWeek.Saturday - (DayOfWeek)dayOfWeek);
    }
}