using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TuitionPlans.ToggleTuitionPlanStatus;

public sealed class ToggleTuitionPlanStatusCommand : ICommand<ToggleTuitionPlanStatusResponse>
{
    public Guid Id { get; init; }
}

