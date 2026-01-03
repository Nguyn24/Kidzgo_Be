using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.CreateProgram;

public sealed class CreateProgramCommand : ICommand<CreateProgramResponse>
{
    public Guid BranchId { get; init; }
    public string Name { get; init; } = null!;
    public string? Level { get; init; }
    public int TotalSessions { get; init; }
    public decimal DefaultTuitionAmount { get; init; }
    public decimal UnitPriceSession { get; init; }
    public string? Description { get; init; }
}

