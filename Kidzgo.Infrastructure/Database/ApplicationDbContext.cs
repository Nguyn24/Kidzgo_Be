using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Payroll;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Audit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IPublisher publisher)
    : DbContext(options), IDbContext
{
    // Users
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<ParentStudentLink> ParentStudentLinks => Set<ParentStudentLink>();

    // Schools
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();

    // Classes
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassEnrollment> ClassEnrollments => Set<ClassEnrollment>();

    // CRM
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadActivity> LeadActivities => Set<LeadActivity>();
    public DbSet<PlacementTest> PlacementTests => Set<PlacementTest>();

    // Exams
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();

    // Finance
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<CashbookEntry> CashbookEntries => Set<CashbookEntry>();

    // Gamification
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<MissionProgress> MissionProgresses => Set<MissionProgress>();
    public DbSet<RewardRedemption> RewardRedemptions => Set<RewardRedemption>();
    public DbSet<RewardStoreItem> RewardStoreItems => Set<RewardStoreItem>();
    public DbSet<StarTransaction> StarTransactions => Set<StarTransaction>();
    public DbSet<StudentLevel> StudentLevels => Set<StudentLevel>();

    // Lesson Plans
    public DbSet<HomeworkAssignment> HomeworkAssignments => Set<HomeworkAssignment>();
    public DbSet<HomeworkStudent> HomeworkStudents => Set<HomeworkStudent>();
    public DbSet<LessonPlan> LessonPlans => Set<LessonPlan>();
    public DbSet<LessonPlanTemplate> LessonPlanTemplates => Set<LessonPlanTemplate>();

    // Media
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    // Payroll
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<PayrollLine> PayrollLines => Set<PayrollLine>();
    public DbSet<PayrollPayment> PayrollPayments => Set<PayrollPayment>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<SessionRole> SessionRoles => Set<SessionRole>();
    public DbSet<ShiftAttendance> ShiftAttendances => Set<ShiftAttendance>();

    // Programs
    public DbSet<Program> Programs => Set<Program>();
    public DbSet<TuitionPlan> TuitionPlans => Set<TuitionPlan>();

    // Reports
    public DbSet<MonthlyReportJob> MonthlyReportJobs => Set<MonthlyReportJob>();
    public DbSet<ReportComment> ReportComments => Set<ReportComment>();
    public DbSet<StudentMonthlyReport> StudentMonthlyReports => Set<StudentMonthlyReport>();

    // Sessions
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<MakeupAllocation> MakeupAllocations => Set<MakeupAllocation>();
    public DbSet<MakeupCredit> MakeupCredits => Set<MakeupCredit>();
    public DbSet<Session> Sessions => Set<Session>();

    // Tickets
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    // Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync();

        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var events = entity.DomainEvents;
                entity.ClearDomainEvents();
                return events;
            })
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent);
        }
    }
}