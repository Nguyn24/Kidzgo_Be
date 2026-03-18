namespace Kidzgo.Application.Registrations.TransferClass;

public sealed class TransferClassResponse
{
    public Guid RegistrationId { get; init; }
    public Guid OldClassId { get; init; }
    public string OldClassName { get; init; } = null!;
    public Guid NewClassId { get; init; }
    public string NewClassName { get; init; } = null!;
    public DateTime EffectiveDate { get; init; }
    public string Status { get; init; } = null!;
}
