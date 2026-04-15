namespace Kidzgo.API.Requests;

public sealed class ReassignEquivalentClassRequest
{
    public Guid RegistrationId { get; set; }
    public Guid NewClassId { get; set; }
    public string Track { get; set; } = "primary";
    public string? SessionSelectionPattern { get; set; }
    public DateTime? EffectiveDate { get; set; }
}
