using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Dashboard;

public sealed class GetDashboardQueryHandler(
    IDbContext context
) : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    public async Task<Result<DashboardResponse>> Handle(GetDashboardQuery query, CancellationToken cancellationToken)
    {
        var startDate = query.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = query.EndDate ?? DateTime.UtcNow;
        var branchId = query.BranchId;

        var response = new DashboardResponse
        {
            Attendance = await GetAttendanceStats(branchId, startDate, endDate, cancellationToken),
            Homework = await GetHomeworkStats(branchId, startDate, endDate, cancellationToken),
            Leave = await GetLeaveStats(branchId, startDate, endDate, cancellationToken),
            MakeupCredits = await GetMakeupCreditStats(branchId, cancellationToken),
            Students = await GetStudentStats(branchId, cancellationToken),
            Enrollments = await GetEnrollmentStats(branchId, startDate, endDate, cancellationToken),
            Leads = await GetLeadStats(branchId, startDate, endDate, cancellationToken),
            PlacementTests = await GetPlacementTestStats(branchId, startDate, endDate, cancellationToken),
            Finance = await GetFinanceStats(branchId, startDate, endDate, cancellationToken),
            HumanResources = await GetHrStats(branchId, startDate, endDate, cancellationToken)
        };

        return Result.Success(response);
    }

    private async Task<AttendanceStats> GetAttendanceStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.Attendances
            .Include(s => s.Session)
            .Where(s => s.Session.PlannedDatetime >= startDate && s.Session.PlannedDatetime <= endDate);

        if (branchId.HasValue)
        {
            query = query.Where(s => s.Session.BranchId == branchId);
        }

        var attendances = await query.ToListAsync(cancellationToken);

        return new AttendanceStats
        {
            TotalSessions = attendances.Count,
            PresentCount = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present),
            AbsentCount = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Absent),
            LateCount = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Makeup),
            AttendanceRate = attendances.Count > 0 
                ? Math.Round((double)attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present) / attendances.Count * 100, 2)
                : 0
        };
    }

    private async Task<HomeworkStats> GetHomeworkStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .ThenInclude(a => a.Class)
            .Where(hs => hs.Assignment.DueAt.HasValue &&
                         hs.Assignment.DueAt.Value >= startDate &&
                         hs.Assignment.DueAt.Value <= endDate);

        if (branchId.HasValue)
        {
            query = query.Where(hs => hs.Assignment.Class.BranchId == branchId);
        }

        var homeworkStudents = await query.ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;

        return new HomeworkStats
        {
            Total = homeworkStudents.Count,
            Pending = homeworkStudents.Count(h => h.Status == HomeworkStatus.Assigned),
            Submitted = homeworkStudents.Count(h => h.Status == HomeworkStatus.Submitted),
            Graded = homeworkStudents.Count(h => h.Status == HomeworkStatus.Graded),
            Overdue = homeworkStudents.Count(h => (h.Status == HomeworkStatus.Assigned || h.Status == HomeworkStatus.Late) &&
                                                  h.Assignment.DueAt.HasValue &&
                                                  h.Assignment.DueAt.Value < now)
        };
    }

    private async Task<LeaveStats> GetLeaveStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var startDateOnly = DateOnly.FromDateTime(startDate);
        var endDateOnly = DateOnly.FromDateTime(endDate);

        var query = context.LeaveRequests
            .Where(l => l.RequestedAt >= startDate && l.RequestedAt <= endDate)
            .Where(l => l.SessionDate >= startDateOnly && l.SessionDate <= endDateOnly);

        if (branchId.HasValue)
        {
            query = query.Where(l => l.Class.BranchId == branchId);
        }

        var leaves = await query.ToListAsync(cancellationToken);

        return new LeaveStats
        {
            Total = leaves.Count,
            Pending = leaves.Count(l => l.Status == LeaveRequestStatus.Pending),
            Approved = leaves.Count(l => l.Status == LeaveRequestStatus.Approved),
            Rejected = leaves.Count(l => l.Status == LeaveRequestStatus.Rejected)
        };
    }

    private async Task<MakeupCreditStats> GetMakeupCreditStats(Guid? branchId, CancellationToken cancellationToken)
    {
        var query = context.MakeupCredits
            .Include(m => m.SourceSession)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(m => m.SourceSession.BranchId == branchId);
        }

        var credits = await query.ToListAsync(cancellationToken);

        return new MakeupCreditStats
        {
            TotalCredits = credits.Count,
            UsedCredits = credits.Count(c => c.Status == MakeupCreditStatus.Used),
            AvailableCredits = credits.Count(c => c.Status == MakeupCreditStatus.Available)
        };
    }

    private async Task<StudentStats> GetStudentStats(Guid? branchId, CancellationToken cancellationToken)
    {
        var query = context.Profiles
            .Where(p => p.ProfileType == ProfileType.Student);

        if (branchId.HasValue)
        {
            query = query.Where(p => p.ClassEnrollments.Any(e => e.Class.BranchId == branchId));
        }

        var students = await query.ToListAsync(cancellationToken);
        var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        return new StudentStats
        {
            Total = students.Count,
            Active = students.Count(p => p.IsActive),
            Inactive = students.Count(p => !p.IsActive),
            NewThisMonth = students.Count(p => p.CreatedAt >= thisMonth)
        };
    }

    private async Task<EnrollmentStats> GetEnrollmentStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.ClassEnrollments.AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(e => e.Class.BranchId == branchId);
        }

        var enrollments = await query.ToListAsync(cancellationToken);
        var thisMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        return new EnrollmentStats
        {
            Total = enrollments.Count,
            Active = enrollments.Count(e => e.Status == EnrollmentStatus.Active),
            Paused = enrollments.Count(e => e.Status == EnrollmentStatus.Paused),
            Dropped = enrollments.Count(e => e.Status == EnrollmentStatus.Dropped),
            NewThisMonth = enrollments.Count(e => e.CreatedAt >= thisMonth)
        };
    }

    private async Task<LeadStats> GetLeadStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.Leads
            .Where(l => l.CreatedAt >= startDate && l.CreatedAt <= endDate);

        if (branchId.HasValue)
        {
            query = query.Where(l => l.BranchPreference == branchId);
        }

        var leads = await query.ToListAsync(cancellationToken);

        var total = leads.Count;
        var enrolled = leads.Count(l => l.Status == LeadStatus.Enrolled);
        var conversionRate = total > 0 ? Math.Round((double)enrolled / total * 100, 2) : 0;

        return new LeadStats
        {
            Total = total,
            New = leads.Count(l => l.Status == LeadStatus.New),
            Contacted = leads.Count(l => l.Status == LeadStatus.Contacted),
            Qualified = leads.Count(l => l.Status == LeadStatus.BookedTest || l.Status == LeadStatus.TestDone),
            Enrolled = enrolled,
            NoShow = 0, // LeadStatus doesn't have NoShow
            Lost = leads.Count(l => l.Status == LeadStatus.Lost),
            ConversionRate = conversionRate,
            TotalTouchCount = leads.Sum(l => l.TouchCount)
        };
    }

    private async Task<PlacementTestStats> GetPlacementTestStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.PlacementTests
            .Where(p => p.ScheduledAt.HasValue &&
                        p.ScheduledAt.Value >= startDate &&
                        p.ScheduledAt.Value <= endDate);

        if (branchId.HasValue)
        {
            query = query.Where(p =>
                (p.Class != null && p.Class.BranchId == branchId) ||
                (p.Lead != null && p.Lead.BranchPreference == branchId));
        }

        var tests = await query.ToListAsync(cancellationToken);

        return new PlacementTestStats
        {
            Total = tests.Count,
            Scheduled = tests.Count(t => t.Status == PlacementTestStatus.Scheduled),
            Completed = tests.Count(t => t.Status == PlacementTestStatus.Completed),
            NoShow = tests.Count(t => t.Status == PlacementTestStatus.NoShow),
            Enrolled = tests.Count(t => t.Status == PlacementTestStatus.Completed) // Need to check if enrolled
        };
    }

    private async Task<FinanceStats> GetFinanceStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var query = context.Invoices
            .Include(i => i.Payments)
            .AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(i => i.BranchId == branchId);
        }

        var invoices = await query.ToListAsync(cancellationToken);

        var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid).ToList();
        var pendingInvoices = invoices.Where(i => i.Status == InvoiceStatus.Pending).ToList();
        var overdueInvoices = invoices.Where(i => i.Status == InvoiceStatus.Overdue).ToList();
        var allPayments = invoices.SelectMany(i => i.Payments).ToList();

        decimal GetOutstanding(IEnumerable<Invoice> selectedInvoices)
        {
            return selectedInvoices.Sum(i => i.Amount - i.Payments.Sum(p => p.Amount));
        }

        return new FinanceStats
        {
            TotalRevenue = allPayments.Sum(p => p.Amount),
            MonthRevenue = allPayments.Where(p => p.PaidAt.HasValue && p.PaidAt.Value >= startDate && p.PaidAt.Value <= endDate).Sum(p => p.Amount),
            PayOSRevenue = allPayments.Where(p => p.Method == PaymentMethod.Payos).Sum(p => p.Amount),
            CashRevenue = allPayments.Where(p => p.Method == PaymentMethod.Cash).Sum(p => p.Amount),
            OutstandingDebt = GetOutstanding(pendingInvoices) + GetOutstanding(overdueInvoices),
            PaidInvoices = paidInvoices.Count,
            PendingInvoices = pendingInvoices.Count,
            OverdueInvoices = overdueInvoices.Count
        };
    }

    private async Task<HrStats> GetHrStats(Guid? branchId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        // Get teachers and staff
        var staffQuery = context.Users.Where(u => u.Role != UserRole.Parent);
        
        if (branchId.HasValue)
        {
            staffQuery = staffQuery.Where(u => u.BranchId == branchId);
        }

        var staff = await staffQuery.ToListAsync(cancellationToken);

        // Get session roles for work hours
        var sessionRolesQuery = context.SessionRoles
            .Include(sr => sr.Session)
            .Where(sr => sr.Session.PlannedDatetime >= startDate && sr.Session.PlannedDatetime <= endDate);

        if (branchId.HasValue)
        {
            sessionRolesQuery = sessionRolesQuery.Where(sr => sr.Session.BranchId == branchId);
        }

        var sessionRoles = await sessionRolesQuery.ToListAsync(cancellationToken);

        // Get payroll runs and payroll payments
        var payrollRunsQuery = context.PayrollRuns.AsQueryable();
        if (branchId.HasValue)
        {
            payrollRunsQuery = payrollRunsQuery.Where(p => p.BranchId == branchId);
        }

        var payrollRuns = await payrollRunsQuery.ToListAsync(cancellationToken);

        var payrollPaymentsQuery = context.PayrollPayments
            .Where(p => p.PaidAt.HasValue && p.PaidAt.Value >= startDate && p.PaidAt.Value <= endDate);
        if (branchId.HasValue)
        {
            payrollPaymentsQuery = payrollPaymentsQuery.Where(p => p.PayrollRun.BranchId == branchId);
        }

        var payrollPayments = await payrollPaymentsQuery.ToListAsync(cancellationToken);

        return new HrStats
        {
            TotalStaff = staff.Count,
            TotalWorkHours = sessionRoles.Sum(sr => sr.Session.DurationMinutes) / 60d,
            TotalPayroll = payrollPayments.Sum(p => p.Amount),
            PayrollProcessed = payrollRuns.Count(p => p.Status == PayrollRunStatus.Approved || p.Status == PayrollRunStatus.Paid),
            PayrollPending = payrollRuns.Count(p => p.Status == PayrollRunStatus.Draft)
        };
    }
}

