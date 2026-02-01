using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.GetTickets;

public sealed class GetTicketsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTicketsQuery, GetTicketsResponse>
{
    public async Task<Result<GetTicketsResponse>> Handle(GetTicketsQuery query, CancellationToken cancellationToken)
    {
        var ticketsQuery = context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.OpenedByProfile)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TicketComments)
            .AsQueryable();

        // Filter by mine (tickets of current user)
        if (query.Mine == true)
        {
            var currentUserId = userContext.UserId;
            ticketsQuery = ticketsQuery.Where(t => 
                t.OpenedByUserId == currentUserId || 
                t.AssignedToUserId == currentUserId);
        }

        // Filter by branch
        if (query.BranchId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.BranchId == query.BranchId.Value);
        }

        // Filter by opened by user
        if (query.OpenedByUserId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.OpenedByUserId == query.OpenedByUserId.Value);
        }

        // Filter by assigned to user
        if (query.AssignedToUserId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.AssignedToUserId == query.AssignedToUserId.Value);
        }

        // Filter by status
        if (query.Status.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.Status == query.Status.Value);
        }

        // Filter by category
        if (query.Category.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.Category == query.Category.Value);
        }

        // Filter by class
        if (query.ClassId.HasValue)
        {
            ticketsQuery = ticketsQuery.Where(t => t.ClassId == query.ClassId.Value);
        }

        // Get total count
        int totalCount = await ticketsQuery.CountAsync(cancellationToken);

        // Apply pagination
        var tickets = await ticketsQuery
            .OrderByDescending(t => t.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(t => new TicketDto
            {
                Id = t.Id,
                OpenedByUserId = t.OpenedByUserId,
                OpenedByUserName = t.OpenedByUser.Name,
                OpenedByProfileId = t.OpenedByProfileId,
                OpenedByProfileName = t.OpenedByProfile != null ? t.OpenedByProfile.DisplayName : null,
                BranchId = t.BranchId,
                BranchName = t.Branch.Name,
                ClassId = t.ClassId,
                ClassCode = t.Class != null ? t.Class.Code : null,
                ClassTitle = t.Class != null ? t.Class.Title : null,
                Category = t.Category.ToString(),
                Subject = t.Subject,
                Message = t.Message,
                Status = t.Status.ToString(),
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUserName = t.AssignedToUser != null ? t.AssignedToUser.Name : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CommentCount = t.TicketComments.Count
            })
            .ToListAsync(cancellationToken);

        var page = new Page<TicketDto>(
            tickets,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetTicketsResponse
        {
            Tickets = page
        };
    }
}

