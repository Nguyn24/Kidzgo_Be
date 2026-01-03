using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TuitionPlans.DeleteTuitionPlan;

public sealed class DeleteTuitionPlanCommand : ICommand
{
    public Guid Id { get; init; }
}

