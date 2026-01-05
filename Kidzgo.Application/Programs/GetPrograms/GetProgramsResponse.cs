using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Programs.GetPrograms;

public sealed class GetProgramsResponse
{
    public Page<ProgramDto> Programs { get; init; } = null!;
}

public sealed class ProgramDto
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Level { get; init; }
    public int TotalSessions { get; init; }
    public decimal DefaultTuitionAmount { get; init; }
    public decimal UnitPriceSession { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

