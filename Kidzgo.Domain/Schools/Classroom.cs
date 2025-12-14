using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Domain.Schools;

public class Classroom : Entity
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = null!;
    public int Capacity { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public Branch Branch { get; set; } = null!;
    public ICollection<Session> PlannedRoomSessions { get; set; } = new List<Session>();
    public ICollection<Session> ActualRoomSessions { get; set; } = new List<Session>();
}
