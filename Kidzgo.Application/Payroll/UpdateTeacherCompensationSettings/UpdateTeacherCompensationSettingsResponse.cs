namespace Kidzgo.Application.Payroll.UpdateTeacherCompensationSettings;

public sealed class UpdateTeacherCompensationSettingsResponse
{
    public int StandardSessionDurationMinutes { get; init; }
    public decimal ForeignTeacherDefaultSessionRate { get; init; }
    public decimal VietnameseTeacherDefaultSessionRate { get; init; }
    public decimal AssistantDefaultSessionRate { get; init; }
}
