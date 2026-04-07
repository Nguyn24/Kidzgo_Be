namespace Kidzgo.API.Requests;

public sealed class GenerateSessionsFromPatternRequest
{
    public Guid ClassId { get; set; }

    /// Neu true, chi generate cac sessions tu hien tai tro di.
    /// Neu false, generate tat ca tu StartDate cua Class.
    public bool OnlyFutureSessions { get; set; } = true;
}
