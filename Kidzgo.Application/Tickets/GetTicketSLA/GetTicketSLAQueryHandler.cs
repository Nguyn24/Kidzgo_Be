using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Tickets.GetTicketSLA;

public sealed class GetTicketSLAQueryHandler(
    IDbContext context
) : IQueryHandler<GetTicketSLAQuery, GetTicketSLAResponse>
{
    public async Task<Result<GetTicketSLAResponse>> Handle(GetTicketSLAQuery query, CancellationToken cancellationToken)
    {
        var ticket = await context.Tickets
            .Include(t => t.TicketComments)
                .ThenInclude(c => c.CommenterUser)
            .FirstOrDefaultAsync(t => t.Id == query.TicketId, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<GetTicketSLAResponse>(TicketErrors.NotFound(query.TicketId));
        }

        // Calculate first response time (time from ticket creation to first comment by ManagementStaff/Teacher)
        DateTime? firstResponseAt = null;
        var firstStaffComment = ticket.TicketComments
            .Where(c => c.CommenterUser.Role == Domain.Users.UserRole.ManagementStaff || 
                       c.CommenterUser.Role == Domain.Users.UserRole.Teacher)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefault();

        if (firstStaffComment != null)
        {
            firstResponseAt = firstStaffComment.CreatedAt;
        }

        // SLA target: First response within 24 hours
        var slaTargetHours = 24;
        var timeToFirstResponse = firstResponseAt.HasValue
            ? (firstResponseAt.Value - ticket.CreatedAt).TotalHours
            : (double?)null;

        var isSLACompliant = timeToFirstResponse.HasValue
            ? timeToFirstResponse.Value <= slaTargetHours
            : (bool?)null;

        var isSLAOverdue = !firstResponseAt.HasValue && 
            (DateTime.UtcNow - ticket.CreatedAt).TotalHours > slaTargetHours;

        var staffCommentCount = ticket.TicketComments
            .Count(c => c.CommenterUser.Role == Domain.Users.UserRole.ManagementStaff || 
                       c.CommenterUser.Role == Domain.Users.UserRole.Teacher);

        return new GetTicketSLAResponse
        {
            TicketId = ticket.Id,
            TicketStatus = ticket.Status,
            CreatedAt = ticket.CreatedAt,
            FirstResponseAt = firstResponseAt,
            TimeToFirstResponseHours = timeToFirstResponse,
            SLATargetHours = slaTargetHours,
            IsSLACompliant = isSLACompliant,
            IsSLAOverdue = isSLAOverdue,
            TotalComments = ticket.TicketComments.Count,
            StaffCommentCount = staffCommentCount
        };
    }
}

