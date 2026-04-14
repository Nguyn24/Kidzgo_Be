using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.IncidentReports.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Tickets.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.IncidentReports.GetIncidentReportById;

public sealed class GetIncidentReportByIdQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetIncidentReportByIdQuery, IncidentReportDetailDto>
{
    public async Task<Result<IncidentReportDetailDto>> Handle(GetIncidentReportByIdQuery query, CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<IncidentReportDetailDto>(TicketErrors.UserNotFound);
        }

        var ticket = await context.Tickets
            .Include(t => t.OpenedByUser)
            .Include(t => t.Branch)
            .Include(t => t.Class)
            .Include(t => t.AssignedToUser)
            .Include(t => t.TicketComments)
                .ThenInclude(c => c.CommenterUser)
            .FirstOrDefaultAsync(t => t.Id == query.Id && t.IsIncidentReport, cancellationToken);

        if (ticket is null)
        {
            return Result.Failure<IncidentReportDetailDto>(IncidentReportErrors.NotFound(query.Id));
        }

        if (!IncidentReportAccessPolicy.CanView(currentUser, ticket))
        {
            return Result.Failure<IncidentReportDetailDto>(IncidentReportErrors.Unauthorized);
        }

        return IncidentReportMapper.ToDetailDto(ticket);
    }
}
