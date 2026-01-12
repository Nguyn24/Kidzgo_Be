using Microsoft.EntityFrameworkCore;
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

namespace Kidzgo.Application.Abstraction.Data;

public interface IDbContext
{
    // Users
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<ParentPinResetToken> ParentPinResetTokens { get; }

    DbSet<Profile> Profiles { get; }
    DbSet<ParentStudentLink> ParentStudentLinks { get; }

    // Schools
    DbSet<Branch> Branches { get; }
    DbSet<Classroom> Classrooms { get; }

    // Classes
    DbSet<Class> Classes { get; }
    DbSet<ClassEnrollment> ClassEnrollments { get; }

    // CRM
    DbSet<Lead> Leads { get; }
    DbSet<LeadActivity> LeadActivities { get; }
    DbSet<PlacementTest> PlacementTests { get; }

    // Exams
    DbSet<Exam> Exams { get; }
    DbSet<ExamResult> ExamResults { get; }
    DbSet<ExamQuestion> ExamQuestions { get; }
    DbSet<ExamSubmission> ExamSubmissions { get; }
    DbSet<ExamSubmissionAnswer> ExamSubmissionAnswers { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<ExerciseQuestion> ExerciseQuestions { get; }
    DbSet<ExerciseSubmission> ExerciseSubmissions { get; }
    DbSet<ExerciseSubmissionAnswer> ExerciseSubmissionAnswers { get; }

    // Finance
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLine> InvoiceLines { get; }
    DbSet<Payment> Payments { get; }
    DbSet<CashbookEntry> CashbookEntries { get; }

    // Gamification
    DbSet<Mission> Missions { get; }
    DbSet<MissionProgress> MissionProgresses { get; }
    DbSet<RewardRedemption> RewardRedemptions { get; }
    DbSet<RewardStoreItem> RewardStoreItems { get; }
    DbSet<StarTransaction> StarTransactions { get; }
    DbSet<StudentLevel> StudentLevels { get; }

    // Lesson Plans
    DbSet<HomeworkAssignment> HomeworkAssignments { get; }
    DbSet<HomeworkStudent> HomeworkStudents { get; }
    DbSet<LessonPlan> LessonPlans { get; }
    DbSet<LessonPlanTemplate> LessonPlanTemplates { get; }

    // Media
    DbSet<MediaAsset> MediaAssets { get; }
    DbSet<Blog> Blogs { get; }

    // Notifications
    DbSet<Notification> Notifications { get; }
    DbSet<NotificationTemplate> NotificationTemplates { get; }
    DbSet<EmailTemplate> EmailTemplates { get; }

    // Payroll
    DbSet<Contract> Contracts { get; }
    DbSet<PayrollLine> PayrollLines { get; }
    DbSet<PayrollPayment> PayrollPayments { get; }
    DbSet<PayrollRun> PayrollRuns { get; }
    DbSet<SessionRole> SessionRoles { get; }
    DbSet<ShiftAttendance> ShiftAttendances { get; }

    // Programs
    DbSet<Program> Programs { get; }
    DbSet<TuitionPlan> TuitionPlans { get; }

    // Reports
    DbSet<MonthlyReportJob> MonthlyReportJobs { get; }
    DbSet<ReportComment> ReportComments { get; }
    DbSet<StudentMonthlyReport> StudentMonthlyReports { get; }

    // Sessions
    DbSet<Domain.Sessions.Attendance> Attendances { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<MakeupAllocation> MakeupAllocations { get; }
    DbSet<MakeupCredit> MakeupCredits { get; }
    DbSet<Session> Sessions { get; }

    // Tickets
    DbSet<Ticket> Tickets { get; }
    DbSet<TicketComment> TicketComments { get; }

    // Audit
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}