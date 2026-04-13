namespace Kidzgo.Domain.Payroll;

public class TeacherCompensationSettings
{
    public int Id { get; set; }
    public int StandardSessionDurationMinutes { get; set; } = 90;
    public decimal ForeignTeacherDefaultSessionRate { get; set; }
    public decimal VietnameseTeacherDefaultSessionRate { get; set; }
    public decimal AssistantDefaultSessionRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
