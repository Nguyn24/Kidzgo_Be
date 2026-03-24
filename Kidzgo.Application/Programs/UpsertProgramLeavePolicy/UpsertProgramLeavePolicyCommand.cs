using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.UpsertProgramLeavePolicy;

public sealed class UpsertProgramLeavePolicyCommand : ICommand<UpsertProgramLeavePolicyResponse>
{
    public Guid ProgramId { get; init; }
    public int MaxLeavesPerMonth { get; init; }
}
