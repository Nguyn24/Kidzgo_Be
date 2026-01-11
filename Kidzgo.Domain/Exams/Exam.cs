using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Exams;

public class Exam : Entity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public ExamType ExamType { get; set; }
    public DateOnly Date { get; set; }
    public decimal? MaxScore { get; set; }
    public string? Description { get; set; }
    
    // Thời gian thi (cho thi tại trung tâm)
    public DateTime? ScheduledStartTime { get; set; } // Thời gian bắt đầu thi (theo lịch)
    public int? TimeLimitMinutes { get; set; } // Thời gian làm bài (phút)
    public bool AllowLateStart { get; set; } // Cho phép bắt đầu muộn (nếu học sinh đến muộn)
    public int? LateStartToleranceMinutes { get; set; } // Cho phép muộn tối đa bao nhiêu phút
    
    // Settings
    public bool AutoSubmitOnTimeLimit { get; set; } // Tự động nộp khi hết giờ
    public bool PreventCopyPaste { get; set; } // Prevent copy/paste (frontend)
    public bool PreventNavigation { get; set; } // Prevent mở tab khác (frontend)
    public bool ShowResultsImmediately { get; set; } // Hiển thị kết quả ngay sau khi nộp (cho multiple choice)
    
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Class Class { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    public ICollection<ExamQuestion> Questions { get; set; } = new List<ExamQuestion>();
    public ICollection<ExamSubmission> Submissions { get; set; } = new List<ExamSubmission>();
}
