using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Domain.Tickets;

public class Ticket : Entity
{
    public Guid Id { get; set; }
    public Guid OpenedByUserId { get; set; }
    public Guid? OpenedByProfileId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ClassId { get; set; }
    public TicketCategory Category { get; set; }
    public string Subject { get; set; } = null!;
    public string Message { get; set; } = null!;
    public TicketStatus Status { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User OpenedByUser { get; set; } = null!;
    public Profile? OpenedByProfile { get; set; }
    public Branch Branch { get; set; } = null!;
    public Class? Class { get; set; }
    public User? AssignedToUser { get; set; }
    public ICollection<TicketComment> TicketComments { get; set; } = new List<TicketComment>();
}
