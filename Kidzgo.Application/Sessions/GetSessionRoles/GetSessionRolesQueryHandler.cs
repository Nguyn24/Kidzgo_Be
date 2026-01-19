using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetSessionRoles;

public sealed class GetSessionRolesQueryHandler(
    IDbContext context
) : IQueryHandler<GetSessionRolesQuery, GetSessionRolesResponse>
{
    public async Task<Result<GetSessionRolesResponse>> Handle(
        GetSessionRolesQuery query,
        CancellationToken cancellationToken)
    {
        // Check if session exists
        bool sessionExists = await context.Sessions
            .AnyAsync(s => s.Id == query.SessionId, cancellationToken);

        if (!sessionExists)
        {
            return Result.Failure<GetSessionRolesResponse>(
                SessionErrors.NotFound(query.SessionId));
        }

        var roles = await context.SessionRoles
            .Where(sr => sr.SessionId == query.SessionId)
            .Include(sr => sr.StaffUser)
            .Select(sr => new SessionRoleDto
            {
                Id = sr.Id,
                SessionId = sr.SessionId,
                StaffUserId = sr.StaffUserId,
                StaffUserName = sr.StaffUser.Name,
                RoleType = sr.RoleType.ToString(),
                PayableUnitPrice = sr.PayableUnitPrice,
                PayableAllowance = sr.PayableAllowance
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetSessionRolesResponse
        {
            Roles = roles
        });
    }
}