namespace Kidzgo.API.Requests;

public sealed class CreateProgramRequest
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = null!;
    public string? Level { get; set; }
    public int TotalSessions { get; set; }
    public decimal DefaultTuitionAmount { get; set; }
    public decimal UnitPriceSession { get; set; }
    public string? Description { get; set; }
}

