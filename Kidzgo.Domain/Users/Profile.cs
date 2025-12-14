using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Audit;

namespace Kidzgo.Domain.Users;

public class Profile : Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ProfileType ProfileType { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? PinHash { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<ParentStudentLink> ParentLinks { get; set; } = new List<ParentStudentLink>();
    public ICollection<ParentStudentLink> StudentLinks { get; set; } = new List<ParentStudentLink>();
    public ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<MakeupCredit> MakeupCredits { get; set; } = new List<MakeupCredit>();
    public ICollection<HomeworkStudent> HomeworkStudents { get; set; } = new List<HomeworkStudent>();
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    public ICollection<StudentMonthlyReport> StudentMonthlyReports { get; set; } = new List<StudentMonthlyReport>();
    public ICollection<MissionProgress> MissionProgresses { get; set; } = new List<MissionProgress>();
    public ICollection<StarTransaction> StarTransactions { get; set; } = new List<StarTransaction>();
    public StudentLevel? StudentLevel { get; set; }
    public ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();
    public ICollection<PlacementTest> PlacementTests { get; set; } = new List<PlacementTest>();
    public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Lead> ConvertedLeads { get; set; } = new List<Lead>();
    public ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();
    public ICollection<Ticket> OpenedTickets { get; set; } = new List<Ticket>();
    public ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
