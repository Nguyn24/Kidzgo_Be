using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.GetTicketById;

public sealed class GetTicketByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetTicketByIdQuery, GetTicketByIdResponse>
{
    public async Task<Result<GetTicketByIdResponse>> Handle(GetTicketByIdQuery query, CancellationToken cancellationToken)
    {
        var ticket = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.OpenedByProfile)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TicketComments)
                .ThenInclude(c => c.CommenterUser)
            .Include(t => t.TicketComments)
                .ThenInclude(c => c.CommenterProfile)
            .FirstOrDefaultAsync(t => t.Id == query.Id, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<GetTicketByIdResponse>(TicketErrors.NotFound(query.Id));
        }

        return new GetTicketByIdResponse
        {
            Id = ticket.Id,
            OpenedByUserId = ticket.OpenedByUserId,
            OpenedByUserName = ticket.OpenedByUser.Name,
            OpenedByProfileId = ticket.OpenedByProfileId,
            OpenedByProfileName = ticket.OpenedByProfile?.DisplayName,
            BranchId = ticket.BranchId,
            BranchName = ticket.Branch.Name,
            ClassId = ticket.ClassId,
            ClassCode = ticket.Class?.Code,
            ClassTitle = ticket.Class?.Title,
            Category = ticket.Category,
            Subject = ticket.Subject,
            Message = ticket.Message,
            Status = ticket.Status,
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = ticket.AssignedToUser?.Name,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            Comments = ticket.TicketComments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new TicketCommentDto
                {
                    Id = c.Id,
                    CommenterUserId = c.CommenterUserId,
                    CommenterUserName = c.CommenterUser.Name,
                    CommenterProfileId = c.CommenterProfileId,
                    CommenterProfileName = c.CommenterProfile != null ? c.CommenterProfile.DisplayName : null,
                    Message = c.Message,
                    AttachmentUrl = c.AttachmentUrl,
                    CreatedAt = c.CreatedAt
                })
                .ToList()
        };
    }
}

