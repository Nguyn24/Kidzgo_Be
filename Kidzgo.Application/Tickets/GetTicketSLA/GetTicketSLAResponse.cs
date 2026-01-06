using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.Tickets.GetTicketSLA;

public sealed class GetTicketSLAResponse
{
    public Guid TicketId { get; init; }
    public TicketStatus TicketStatus { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? FirstResponseAt { get; init; }
    public double? TimeToFirstResponseHours { get; init; }
    public int SLATargetHours { get; init; }
    public bool? IsSLACompliant { get; init; }
    public bool IsSLAOverdue { get; init; }
    public int TotalComments { get; init; }
    public int StaffCommentCount { get; init; }
}

