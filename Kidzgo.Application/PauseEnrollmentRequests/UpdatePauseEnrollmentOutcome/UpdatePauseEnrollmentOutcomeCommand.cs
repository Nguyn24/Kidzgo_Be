using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.PauseEnrollmentRequests.UpdatePauseEnrollmentOutcome;

public sealed class UpdatePauseEnrollmentOutcomeCommand : ICommand
{
    public Guid Id { get; init; }
    public PauseEnrollmentOutcome Outcome { get; init; }
    public string? OutcomeNote { get; init; }
}
