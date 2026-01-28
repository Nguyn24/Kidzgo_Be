using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.SuggestSessions;

public sealed class SuggestMakeupSessionsQueryHandler(IDbContext context)
    : IQueryHandler<SuggestMakeupSessionsQuery, IEnumerable<SuggestedSessionResponse>>
{
    public async Task<Result<IEnumerable<SuggestedSessionResponse>>> Handle(
        SuggestMakeupSessionsQuery query,
        CancellationToken cancellationToken)
    {
        var credit = await context.MakeupCredits
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.MakeupCreditId, cancellationToken);

        if (credit is null)
        {
            return Result.Failure<IEnumerable<SuggestedSessionResponse>>(MakeupCreditErrors.NotFound(query.MakeupCreditId));
        }

        // Lấy thông tin buổi học nguồn cùng với lớp và chương trình để xác định trình độ
        var sourceSession = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .ThenInclude(c => c.Program)
            .FirstOrDefaultAsync(s => s.Id == credit.SourceSessionId, cancellationToken);

        if (sourceSession is null)
        {
            return Result.Failure<IEnumerable<SuggestedSessionResponse>>(MakeupCreditErrors.NotFound(credit.SourceSessionId));
        }

        var now = DateTime.UtcNow;
        var sourceProgram = sourceSession.Class.Program;

        // Lấy danh sách các khoảng thời gian (start-end) của tất cả session mà học sinh đang học
        // để tránh gợi ý các buổi bị trùng hoặc quá sát giờ (cách nhau < 2 tiếng)
        var studentSessionTimes = await context.Sessions
            .AsNoTracking()
            .Where(s => s.Class.ClassEnrollments
                .Any(ce => ce.StudentProfileId == credit.StudentProfileId &&
                           ce.Status == Domain.Classes.EnrollmentStatus.Active))
            .Where(s => s.Status == SessionStatus.Scheduled)
            .Select(s => new
            {
                Start = s.PlannedDatetime,
                End = s.PlannedDatetime.AddMinutes(s.DurationMinutes)
            })
            .ToListAsync(cancellationToken);

        // Gợi ý các buổi học bù:
        // - Cùng trình độ (cùng Program.Level)
        // - Cùng chi nhánh
        // - Khác lớp với buổi nguồn
        // - Trạng thái Scheduled và thời gian trong tương lai
        // - Không trùng/ quá sát giờ với các buổi mà học sinh đang học (cách nhau tối thiểu 2 tiếng)
        var rawSuggestions = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .ThenInclude(c => c.Program)
            .Where(s => s.Id != sourceSession.Id)
            .Where(s => s.Status == SessionStatus.Scheduled && s.PlannedDatetime >= now)
            .Where(s => s.BranchId == sourceSession.BranchId)
            .Where(s => s.Class.Program.Level == sourceProgram.Level)
            .Where(s => s.ClassId != sourceSession.ClassId)
            .OrderBy(s => s.PlannedDatetime)
            .ToListAsync(cancellationToken);

        var minGap = TimeSpan.FromHours(2);

        var filtered = rawSuggestions
            .Where(s =>
            {
                var start = s.PlannedDatetime;
                var end = s.PlannedDatetime.AddMinutes(s.DurationMinutes);

                // Không chọn nếu khoảng thời gian này giao với bất kỳ session nào của học sinh
                // hoặc khoảng cách giữa hai buổi < 2 tiếng
                return !studentSessionTimes.Any(st =>
                {
                    // Điều kiện giao nhau (overlap)
                    bool overlap = start < st.End && end > st.Start;

                    // Khoảng cách tối thiểu 2 tiếng giữa các buổi (kể cả không overlap)
                    var gapToStart = (start - st.End).Duration();
                    var gapToEnd = (st.Start - end).Duration();
                    bool tooClose = gapToStart < minGap || gapToEnd < minGap;

                    return overlap || tooClose;
                });
            })
            .Take(query.Limit)
            .Select(s => new SuggestedSessionResponse
            {
                SessionId = s.Id,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
                ClassTitle = s.Class.Title,
                ProgramName = s.Class.Program.Name,
                ProgramLevel = s.Class.Program.Level,
                PlannedDatetime = s.PlannedDatetime,
                PlannedEndDatetime = s.PlannedDatetime.AddMinutes(s.DurationMinutes),
                BranchId = s.BranchId
            })
            .ToList();

        return filtered;
    }
}