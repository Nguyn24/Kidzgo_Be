using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Tickets;

public class TicketComment : Entity
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid CommenterUserId { get; set; }
    public Guid? CommenterProfileId { get; set; }
    public string Message { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Ticket Ticket { get; set; } = null!;
    public User CommenterUser { get; set; } = null!;
    public Profile? CommenterProfile { get; set; }
}
