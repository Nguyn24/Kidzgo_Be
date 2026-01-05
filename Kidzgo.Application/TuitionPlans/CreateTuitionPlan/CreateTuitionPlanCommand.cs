using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TuitionPlans.CreateTuitionPlan;

public sealed class CreateTuitionPlanCommand : ICommand<CreateTuitionPlanResponse>
{
    public Guid? BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public string Name { get; init; } = null!;
    public int TotalSessions { get; init; }
    public decimal TuitionAmount { get; init; }
    public decimal UnitPriceSession { get; init; }
    public string Currency { get; init; } = null!;
}

