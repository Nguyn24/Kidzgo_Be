namespace Kidzgo.Application.Registrations.CancelRegistration;

public sealed class CancelRegistrationResponse
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
    public string? Reason { get; init; }
    public DateTime CancelledAt { get; init; }
}
