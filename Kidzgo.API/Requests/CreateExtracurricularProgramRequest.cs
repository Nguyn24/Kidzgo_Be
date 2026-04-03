namespace Kidzgo.API.Requests;

public sealed class CreateExtracurricularProgramRequest
{
    public Guid BranchId { get; init; }
    public string Name { get; init; } = null!;
    public string Type { get; init; } = null!;
    public DateOnly Date { get; init; }
    public int Capacity { get; init; }
    public int RegisteredCount { get; init; }
    public decimal Fee { get; init; }
    public string? Location { get; init; }
    public bool IsActive { get; init; } = true;
}
