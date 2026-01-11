using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class MonthlyReportData : Entity
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid StudentProfileId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    
    // Aggregated data stored as JSON
    public string? AttendanceData { get; set; }  // JSON: { total, present, absent, makeup, percentage }
    public string? HomeworkData { get; set; }    // JSON: { total, completed, pending, average, completionRate }
    public string? TestData { get; set; }        // JSON: { tests: [{ type, score, maxScore, date, comment }] }
    public string? MissionData { get; set; }     // JSON: { completed, total, stars, xp, achievements }
    public string? NotesData { get; set; }       // JSON: { sessionReports: [{ sessionId, date, notes, feedback }] }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public StudentMonthlyReport Report { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
}

