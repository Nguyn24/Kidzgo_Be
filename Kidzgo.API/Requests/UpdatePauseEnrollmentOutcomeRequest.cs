using Kidzgo.Domain.Classes;

namespace Kidzgo.API.Requests;

public sealed class UpdatePauseEnrollmentOutcomeRequest
{
    public PauseEnrollmentOutcome Outcome { get; set; }
    public string? OutcomeNote { get; set; }
}
