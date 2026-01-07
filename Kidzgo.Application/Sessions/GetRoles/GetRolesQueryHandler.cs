using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Payroll;

namespace Kidzgo.Application.Sessions.GetRoles;

public sealed class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, GetRolesResponse>
{
    public Task<Result<GetRolesResponse>> Handle(GetRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = Enum.GetValues<SessionRoleType>()
            .Select(r => new RoleDto
            {
                Value = r.ToString(),
                DisplayName = r.ToString()
            })
            .ToList();

        return Task.FromResult<Result<GetRolesResponse>>(
            new GetRolesResponse
            {
                Roles = roles
            });
    }
}

