using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Programs;

public sealed class ProgramLeavePolicy : Entity
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public int MaxLeavesPerMonth { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Program Program { get; set; } = null!;
    public User? UpdatedByUser { get; set; }
}
