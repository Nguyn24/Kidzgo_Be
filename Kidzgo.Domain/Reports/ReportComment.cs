using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class ReportComment : Entity
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }
    public Guid CommenterId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public StudentMonthlyReport Report { get; set; } = null!;
    public User CommenterUser { get; set; } = null!;
}
