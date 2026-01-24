using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.UpdateSessionRole;

public sealed class UpdateSessionRoleCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateSessionRoleCommand, UpdateSessionRoleResponse>
{
    public async Task<Result<UpdateSessionRoleResponse>> Handle(
        UpdateSessionRoleCommand command,
        CancellationToken cancellationToken)
    {
        var sessionRole = await context.SessionRoles
            .FirstOrDefaultAsync(sr => sr.Id == command.SessionRoleId, cancellationToken);

        if (sessionRole is null)
        {
            return Result.Failure<UpdateSessionRoleResponse>(SessionRoleErrors.NotFound(command.SessionRoleId));
        }

        // Update only PayableUnitPrice and PayableAllowance
        if (command.PayableUnitPrice.HasValue)
        {
            sessionRole.PayableUnitPrice = command.PayableUnitPrice.Value;
        }

        if (command.PayableAllowance.HasValue)
        {
            sessionRole.PayableAllowance = command.PayableAllowance.Value;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateSessionRoleResponse
        {
            Id = sessionRole.Id,
            SessionId = sessionRole.SessionId,
            StaffUserId = sessionRole.StaffUserId,
            RoleType = sessionRole.RoleType.ToString(),
            PayableUnitPrice = sessionRole.PayableUnitPrice,
            PayableAllowance = sessionRole.PayableAllowance
        };
    }
}