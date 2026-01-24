using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Payroll;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.CreateSessionRole;

public sealed class CreateSessionRoleCommandHandler(
    IDbContext context
) : ICommandHandler<CreateSessionRoleCommand, CreateSessionRoleResponse>
{
    public async Task<Result<CreateSessionRoleResponse>> Handle(
        CreateSessionRoleCommand command,
        CancellationToken cancellationToken)
    {
        // Check if session exists
        var session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<CreateSessionRoleResponse>(
                SessionErrors.NotFound(command.SessionId));
        }

        // Check if staff user exists and is Teacher or ManagementStaff
        var staffUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.StaffUserId &&
                (u.Role == UserRole.Teacher || u.Role == UserRole.ManagementStaff),
                cancellationToken);

        if (staffUser is null)
        {
            return Result.Failure<CreateSessionRoleResponse>(
                UserErrors.NotFound(command.StaffUserId));
        }

        // Check if session role already exists for this session and staff user
        bool roleExists = await context.SessionRoles
            .AnyAsync(sr => sr.SessionId == command.SessionId &&
                           sr.StaffUserId == command.StaffUserId &&
                           sr.RoleType == command.RoleType,
                           cancellationToken);

        if (roleExists)
        {
            return Result.Failure<CreateSessionRoleResponse>(SessionRoleErrors.AlreadyExists);
        }

        var sessionRole = new SessionRole
        {
            Id = Guid.NewGuid(),
            SessionId = command.SessionId,
            StaffUserId = command.StaffUserId,
            RoleType = command.RoleType,
            PayableUnitPrice = command.PayableUnitPrice,
            PayableAllowance = command.PayableAllowance
        };

        context.SessionRoles.Add(sessionRole);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateSessionRoleResponse
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

