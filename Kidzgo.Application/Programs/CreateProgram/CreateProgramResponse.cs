namespace Kidzgo.Application.Programs.CreateProgram;

public sealed class CreateProgramResponse
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string Name { get; init; } = null!;
    public string Code { get; init; }
    public bool IsMakeup { get; init; }
    public Guid? DefaultMakeupClassId { get; init; }
    public int TotalSessions { get; init; }
    public decimal DefaultTuitionAmount { get; init; }
    public decimal UnitPriceSession { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

