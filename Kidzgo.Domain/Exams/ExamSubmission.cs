using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Exams;

public class ExamSubmission : Entity
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid StudentProfileId { get; set; }
    
    // Thời gian thi
    public DateTime? ActualStartTime { get; set; } // Thời gian học sinh bắt đầu làm (tại trung tâm)
    public DateTime? SubmittedAt { get; set; } // Thời gian nộp bài
    public DateTime? AutoSubmittedAt { get; set; } // Thời gian auto-submit (nếu hết giờ)
    public int? TimeSpentMinutes { get; set; } // Thời gian làm bài thực tế
    
    // Điểm số
    public decimal? AutoScore { get; set; } // Điểm tự động (multiple choice)
    public decimal? FinalScore { get; set; } // Điểm cuối cùng (sau khi teacher chấm)
    
    // Chấm bài
    public Guid? GradedBy { get; set; }
    public DateTime? GradedAt { get; set; }
    public string? TeacherComment { get; set; }
    
    // Trạng thái
    public ExamSubmissionStatus Status { get; set; } // NOT_STARTED, IN_PROGRESS, SUBMITTED, AUTO_SUBMITTED, GRADED

    // Navigation properties
    public Exam Exam { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public User? GradedByUser { get; set; }
    public ICollection<ExamSubmissionAnswer> SubmissionAnswers { get; set; } = new List<ExamSubmissionAnswer>();
}

public enum ExamSubmissionStatus
{
    NotStarted,      // Chưa bắt đầu
    InProgress,      // Đang làm bài
    Submitted,       // Đã nộp (học sinh tự nộp)
    AutoSubmitted,   // Auto-submit (hết giờ)
    Graded          // Đã chấm
}

