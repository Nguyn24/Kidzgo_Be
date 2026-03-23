namespace Kidzgo.Application.Programs.UpsertProgramLeavePolicy;

public sealed class UpsertProgramLeavePolicyResponse
{
    public Guid ProgramId { get; init; }
    public int MaxLeavesPerMonth { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Guid? UpdatedBy { get; init; }
}
