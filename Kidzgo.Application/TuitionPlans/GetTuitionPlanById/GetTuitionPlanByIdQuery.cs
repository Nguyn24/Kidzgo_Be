using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TuitionPlans.GetTuitionPlanById;

public sealed class GetTuitionPlanByIdQuery : IQuery<GetTuitionPlanByIdResponse>
{
    public Guid Id { get; init; }
}

