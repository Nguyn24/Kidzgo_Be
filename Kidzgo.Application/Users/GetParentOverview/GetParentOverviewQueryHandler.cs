using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.GetParentOverview;

public sealed class GetParentOverviewQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetParentOverviewQuery, ParentOverviewResponse>
{
    public async Task<Result<ParentOverviewResponse>> Handle(
        GetParentOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var now = DateTime.UtcNow;
        var fromDate = query.FromDate ?? now.AddMonths(-1);
        var toDate = query.ToDate ?? now.AddMonths(1);

        // Get parent profile
        var parentProfile = await context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && 
                                     p.ProfileType == Domain.Users.ProfileType.Parent &&
                                     !p.IsDeleted && p.IsActive, cancellationToken);

        if (parentProfile == null)
        {
            return Result.Failure<ParentOverviewResponse>(
                Domain.Common.Error.NotFound("ParentProfile", "not found"));
        }

        // Get student profiles linked to this parent
        var studentProfileIds = await context.ParentStudentLinks
            .AsNoTracking()
            .Where(link => link.ParentProfileId == parentProfile.Id)
            .Select(link => link.StudentProfileId)
            .ToListAsync(cancellationToken);

        if (!studentProfileIds.Any())
        {
            return new ParentOverviewResponse
            {
                Statistics = new DashboardStatistics(),
                StudentProfiles = new List<StudentProfileDto>()
            };
        }

        // Filter by specific student if provided
        if (query.StudentProfileId.HasValue && studentProfileIds.Contains(query.StudentProfileId.Value))
        {
            studentProfileIds = new List<Guid> { query.StudentProfileId.Value };
        }

        // Statistics
        var statistics = new DashboardStatistics
        {
            TotalStudents = studentProfileIds.Count,
            TotalClasses = await context.ClassEnrollments
                .AsNoTracking()
                .Where(ce => studentProfileIds.Contains(ce.StudentProfileId) && 
                           ce.Status == EnrollmentStatus.Active)
                .Select(ce => ce.ClassId)
                .Distinct()
                .CountAsync(cancellationToken),
            UpcomingSessions = await context.Sessions
                .AsNoTracking()
                .Where(s => s.Class.ClassEnrollments.Any(ce => 
                    studentProfileIds.Contains(ce.StudentProfileId) &&
                    ce.Status == EnrollmentStatus.Active) &&
                    s.Status == SessionStatus.Scheduled &&
                    s.PlannedDatetime >= now)
                .CountAsync(cancellationToken),
            AvailableMakeupCredits = await context.MakeupCredits
                .AsNoTracking()
                .CountAsync(mc => studentProfileIds.Contains(mc.StudentProfileId) &&
                                 mc.Status == MakeupCreditStatus.Available, cancellationToken),
            PendingHomeworks = 0, // TODO: Implement homework count
            PendingInvoices = await context.Invoices
                .AsNoTracking()
                .CountAsync(i => studentProfileIds.Contains(i.StudentProfileId) &&
                                i.Status == InvoiceStatus.Pending, cancellationToken),
            ActiveMissions = await context.MissionProgresses
                .AsNoTracking()
                .CountAsync(mp => studentProfileIds.Contains(mp.StudentProfileId) &&
                                 (mp.Status == MissionProgressStatus.Assigned || 
                                  mp.Status == MissionProgressStatus.InProgress), cancellationToken),
            TotalStars = await context.StarTransactions
                .AsNoTracking()
                .Where(st => studentProfileIds.Contains(st.StudentProfileId))
                .SumAsync(st => (int?)st.Amount, cancellationToken) ?? 0
        };

        // Student Profiles
        var studentProfiles = await context.Profiles
            .AsNoTracking()
            .Where(p => studentProfileIds.Contains(p.Id))
            .Select(p => new StudentProfileDto
            {
                Id = p.Id,
                DisplayName = p.DisplayName,
                Level = p.StudentLevel != null ? int.Parse(p.StudentLevel.CurrentLevel) : null,
                TotalStars = p.StarTransactions.Sum(st => st.Amount)
            })
            .ToListAsync(cancellationToken);

        // Classes
        var classes = await context.ClassEnrollments
            .AsNoTracking()
            .Where(ce => studentProfileIds.Contains(ce.StudentProfileId) &&
                        ce.Status == EnrollmentStatus.Active)
            .Select(ce => new ClassSummaryDto
            {
                Id = ce.ClassId,
                Code = ce.Class.Code,
                Title = ce.Class.Title,
                StudentProfileId = ce.StudentProfileId,
                Status = ce.Class.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var classIds = classes.Select(c => c.Id).Distinct().ToList();

        // Apply class filter
        if (query.ClassId.HasValue && classIds.Contains(query.ClassId.Value))
        {
            classIds = new List<Guid> { query.ClassId.Value };
        }

        // Upcoming Sessions
        var upcomingSessionsQuery = context.Sessions
            .AsNoTracking()
            .Where(s => classIds.Contains(s.ClassId) &&
                       s.Status == SessionStatus.Scheduled &&
                       s.PlannedDatetime >= now);

        if (query.SessionId.HasValue)
        {
            upcomingSessionsQuery = upcomingSessionsQuery.Where(s => s.Id == query.SessionId.Value);
        }

        var upcomingSessions = await upcomingSessionsQuery
            .OrderBy(s => s.PlannedDatetime)
            .Take(20)
            .Select(s => new SessionSummaryDto
            {
                Id = s.Id,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
                StudentProfileId = studentProfileIds.First(), // TODO: Map correctly
                PlannedDatetime = s.PlannedDatetime,
                Status = s.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Recent Attendances
        var recentAttendances = await context.Attendances
            .AsNoTracking()
            .Where(a => studentProfileIds.Contains(a.StudentProfileId) &&
                       a.Session.PlannedDatetime >= fromDate &&
                       a.Session.PlannedDatetime <= toDate)
            .OrderByDescending(a => a.Session.PlannedDatetime)
            .Take(20)
            .Select(a => new AttendanceSummaryDto
            {
                SessionId = a.SessionId,
                ClassCode = a.Session.Class.Code,
                SessionDate = a.Session.PlannedDatetime,
                AttendanceStatus = a.AttendanceStatus.ToString()
            })
            .ToListAsync(cancellationToken);

        // Makeup Credits
        var makeupCredits = await context.MakeupCredits
            .AsNoTracking()
            .Where(mc => studentProfileIds.Contains(mc.StudentProfileId))
            .OrderByDescending(mc => mc.CreatedAt)
            .Take(20)
            .Select(mc => new MakeupCreditSummaryDto
            {
                Id = mc.Id,
                Status = mc.Status.ToString(),
                ExpiresAt = mc.ExpiresAt,
                UsedSessionId = mc.UsedSessionId
            })
            .ToListAsync(cancellationToken);

        // Pending Homeworks (TODO: Implement when Homework entity is available)
        var pendingHomeworks = new List<HomeworkSummaryDto>();

        // Recent Exams
        var recentExams = await context.ExamResults
            .AsNoTracking()
            .Where(er => studentProfileIds.Contains(er.StudentProfileId) &&
                        er.Exam.Date >= DateOnly.FromDateTime(fromDate))
            .OrderByDescending(er => er.Exam.Date)
            .Take(20)
            .Select(er => new ExamSummaryDto
            {
                Id = er.ExamId,
                Title = er.Exam.Description ?? "Exam",
                ClassId = er.Exam.ClassId,
                ClassCode = er.Exam.Class.Code,
                StudentProfileId = er.StudentProfileId,
                ExamDate = er.Exam.Date.ToDateTime(TimeOnly.MinValue),
                Score = er.Score
            })
            .ToListAsync(cancellationToken);

        // Reports
        var reports = await context.StudentMonthlyReports
            .AsNoTracking()
            .Where(r => studentProfileIds.Contains(r.StudentProfileId) &&
                       r.Status == ReportStatus.Published)
            .OrderByDescending(r => new DateTime(r.Year, r.Month, 1))
            .Take(20)
            .Select(r => new ReportSummaryDto
            {
                Id = r.Id,
                StudentProfileId = r.StudentProfileId,
                ClassCode = r.Class != null ? r.Class.Code : "",
                ReportMonth = new DateTime(r.Year, r.Month, 1),
                Status = r.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        // Pending Invoices
        var pendingInvoices = await context.Invoices
            .AsNoTracking()
            .Where(i => studentProfileIds.Contains(i.StudentProfileId) &&
                       i.Status == InvoiceStatus.Pending)
            .OrderBy(i => i.DueDate)
            .Take(20)
            .Select(i => new InvoiceSummaryDto
            {
                Id = i.Id,
                InvoiceNumber = i.Id.ToString(),
                StudentProfileId = i.StudentProfileId,
                Amount = i.Amount,
                PaymentStatus = i.Status.ToString(),
                DueDate = i.DueDate.HasValue ? i.DueDate.Value.ToDateTime(TimeOnly.MinValue) : null
            })
            .ToListAsync(cancellationToken);

        // Active Missions
        var activeMissions = await context.MissionProgresses
            .AsNoTracking()
            .Where(mp => studentProfileIds.Contains(mp.StudentProfileId) &&
                        (mp.Status == MissionProgressStatus.Assigned ||
                         mp.Status == MissionProgressStatus.InProgress))
            .OrderByDescending(mp => mp.Mission.CreatedAt)
            .Take(20)
            .Select(mp => new MissionSummaryDto
            {
                Id = mp.MissionId,
                Title = mp.Mission.Title,
                StudentProfileId = mp.StudentProfileId,
                Status = mp.Status.ToString(),
                StarReward = mp.Mission.RewardStars ?? 0
            })
            .ToListAsync(cancellationToken);

        // Open Tickets
        var openTickets = await context.Tickets
            .AsNoTracking()
            .Where(t => studentProfileIds.Contains(t.OpenedByProfileId.Value) &&
                       t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .Select(t => new TicketSummaryDto
            {
                Id = t.Id,
                Title = t.Subject,
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new ParentOverviewResponse
        {
            Statistics = statistics,
            StudentProfiles = studentProfiles,
            Classes = classes,
            UpcomingSessions = upcomingSessions,
            RecentAttendances = recentAttendances,
            MakeupCredits = makeupCredits,
            PendingHomeworks = pendingHomeworks,
            RecentExams = recentExams,
            Reports = reports,
            PendingInvoices = pendingInvoices,
            ActiveMissions = activeMissions,
            OpenTickets = openTickets
        };
    }
}

