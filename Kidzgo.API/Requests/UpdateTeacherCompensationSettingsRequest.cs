namespace Kidzgo.API.Requests;

public sealed class UpdateTeacherCompensationSettingsRequest
{
    public int StandardSessionDurationMinutes { get; set; }
    public decimal ForeignTeacherDefaultSessionRate { get; set; }
    public decimal VietnameseTeacherDefaultSessionRate { get; set; }
    public decimal AssistantDefaultSessionRate { get; set; }
}
