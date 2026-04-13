using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Payroll.UpdateTeacherCompensationSettings;

public sealed class UpdateTeacherCompensationSettingsCommand : ICommand<UpdateTeacherCompensationSettingsResponse>
{
    public int StandardSessionDurationMinutes { get; init; }
    public decimal ForeignTeacherDefaultSessionRate { get; init; }
    public decimal VietnameseTeacherDefaultSessionRate { get; init; }
    public decimal AssistantDefaultSessionRate { get; init; }
}
