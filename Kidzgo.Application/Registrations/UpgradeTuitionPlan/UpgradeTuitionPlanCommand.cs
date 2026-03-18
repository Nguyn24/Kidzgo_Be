using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.UpgradeTuitionPlan;

public sealed class UpgradeTuitionPlanCommand : ICommand<UpgradeTuitionPlanResponse>
{
    public Guid RegistrationId { get; init; }
    public Guid NewTuitionPlanId { get; init; }
}
