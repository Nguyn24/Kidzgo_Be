using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.UpdateProgram;

public sealed class UpdateProgramCommand : ICommand<UpdateProgramResponse>
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string Name { get; init; } = null!;
    public string? Level { get; init; }
    public int TotalSessions { get; init; }
    public decimal DefaultTuitionAmount { get; init; }
    public decimal UnitPriceSession { get; init; }
    public string? Description { get; init; }
}

