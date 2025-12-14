using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Exams;

public class ExamResult : Entity
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public Guid StudentProfileId { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
    public string? AttachmentUrl { get; set; }
    public Guid? GradedBy { get; set; }
    public DateTime? GradedAt { get; set; }

    // Navigation properties
    public Exam Exam { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public User? GradedByUser { get; set; }
}
