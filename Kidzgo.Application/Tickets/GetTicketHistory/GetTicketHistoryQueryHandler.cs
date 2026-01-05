using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.GetTicketHistory;

public sealed class GetTicketHistoryQueryHandler(
    IDbContext context
) : IQueryHandler<GetTicketHistoryQuery, GetTicketHistoryResponse>
{
    public async Task<Result<GetTicketHistoryResponse>> Handle(GetTicketHistoryQuery query, CancellationToken cancellationToken)
    {
        var ticket = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.OpenedByProfile)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == query.TicketId, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<GetTicketHistoryResponse>(
                Error.NotFound("Ticket.NotFound", "Ticket not found"));
        }

        var comments = await context.TicketComments
            .Include(c => c.CommenterUser)
            .Include(c => c.CommenterProfile)
            .Where(c => c.TicketId == query.TicketId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new TicketHistoryItemDto
            {
                Type = "Comment",
                CommentId = c.Id,
                CommenterUserId = c.CommenterUserId,
                CommenterUserName = c.CommenterUser.Name,
                CommenterProfileId = c.CommenterProfileId,
                CommenterProfileName = c.CommenterProfile != null ? c.CommenterProfile.DisplayName : null,
                Message = c.Message,
                AttachmentUrl = c.AttachmentUrl,
                Timestamp = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Add ticket creation as first history item
        var history = new List<TicketHistoryItemDto>
        {
            new TicketHistoryItemDto
            {
                Type = "Created",
                CommentId = null,
                CommenterUserId = ticket.OpenedByUserId,
                CommenterUserName = ticket.OpenedByUser.Name,
                CommenterProfileId = ticket.OpenedByProfileId,
                CommenterProfileName = ticket.OpenedByProfile?.DisplayName,
                Message = $"Ticket created: {ticket.Message}",
                AttachmentUrl = null,
                Timestamp = ticket.CreatedAt
            }
        };

        // Add status changes (simplified - in real app, you might want to track status changes separately)
        if (ticket.Status != Domain.Tickets.TicketStatus.Open)
        {
            history.Add(new TicketHistoryItemDto
            {
                Type = "StatusChanged",
                CommentId = null,
                CommenterUserId = ticket.UpdatedAt == ticket.CreatedAt ? ticket.OpenedByUserId : Guid.Empty,
                CommenterUserName = ticket.UpdatedAt == ticket.CreatedAt ? ticket.OpenedByUser.Name : "System",
                CommenterProfileId = null,
                CommenterProfileName = null,
                Message = $"Status changed to: {ticket.Status}",
                AttachmentUrl = null,
                Timestamp = ticket.UpdatedAt
            });
        }

        // Add assignment if assigned
        if (ticket.AssignedToUserId.HasValue)
        {
            history.Add(new TicketHistoryItemDto
            {
                Type = "Assigned",
                CommentId = null,
                CommenterUserId = ticket.AssignedToUserId.Value,
                CommenterUserName = ticket.AssignedToUser?.Name ?? "Unknown",
                CommenterProfileId = null,
                CommenterProfileName = null,
                Message = $"Ticket assigned to {ticket.AssignedToUser?.Name}",
                AttachmentUrl = null,
                Timestamp = ticket.UpdatedAt
            });
        }

        history.AddRange(comments);
        history = history.OrderBy(h => h.Timestamp).ToList();

        return new GetTicketHistoryResponse
        {
            TicketId = ticket.Id,
            TicketStatus = ticket.Status,
            History = history
        };
    }
}

