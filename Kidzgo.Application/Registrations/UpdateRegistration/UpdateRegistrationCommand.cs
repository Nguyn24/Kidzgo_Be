using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.UpdateRegistration;

public sealed class UpdateRegistrationCommand : ICommand<UpdateRegistrationResponse>
{
    public Guid Id { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public string? Note { get; init; }
    public Guid? TuitionPlanId { get; init; }
}
