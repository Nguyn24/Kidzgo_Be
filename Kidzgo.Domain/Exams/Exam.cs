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
    public Guid? CreatedBy { get; set; }

    // Navigation properties
    public Class Class { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}
