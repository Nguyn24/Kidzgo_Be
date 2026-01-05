using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Enrollments.AssignTuitionPlan;

public sealed class AssignTuitionPlanCommand : ICommand<AssignTuitionPlanResponse>
{
    public Guid Id { get; init; }
    public Guid TuitionPlanId { get; init; }
}

