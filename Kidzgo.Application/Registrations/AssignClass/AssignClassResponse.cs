namespace Kidzgo.Application.Registrations.AssignClass;

public sealed class AssignClassResponse
{
    public Guid RegistrationId { get; init; }
    public string RegistrationStatus { get; init; } = null!;
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public string Track { get; init; } = null!;
    public string EntryType { get; init; } = null!;
    public DateTime ClassAssignedDate { get; init; }
    public string? WarningMessage { get; init; }
}
