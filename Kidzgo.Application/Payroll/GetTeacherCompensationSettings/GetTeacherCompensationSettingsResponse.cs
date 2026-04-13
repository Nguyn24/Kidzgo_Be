namespace Kidzgo.Application.Payroll.GetTeacherCompensationSettings;

public sealed class GetTeacherCompensationSettingsResponse
{
    public int StandardSessionDurationMinutes { get; init; }
    public decimal ForeignTeacherDefaultSessionRate { get; init; }
    public decimal VietnameseTeacherDefaultSessionRate { get; init; }
    public decimal AssistantDefaultSessionRate { get; init; }
}
