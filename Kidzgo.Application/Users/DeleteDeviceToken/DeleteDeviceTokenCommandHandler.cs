using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.DeleteDeviceToken;

public sealed class DeleteDeviceTokenCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<DeleteDeviceTokenCommand, DeleteTokenResponse>
{
    public async Task<Result<DeleteTokenResponse>> Handle(
        DeleteDeviceTokenCommand command,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        // Find device token(s) to delete
        IQueryable<DeviceToken> query = context.DeviceTokens
            .Where(dt => dt.UserId == userId && dt.IsActive);

        if (!string.IsNullOrWhiteSpace(command.Token))
        {
            query = query.Where(dt => dt.Token == command.Token);
        }
        else if (command.DeviceId.HasValue)
        {
            query = query.Where(dt => dt.DeviceId == command.DeviceId.Value.ToString());
        }
        else
        {
            // If neither token nor deviceId provided, return error
            return new DeleteTokenResponse
            {
                Success = false,
                Message = "Either token or deviceId must be provided"
            };
        }

        var deviceTokens = await query.ToListAsync(cancellationToken);

        if (deviceTokens.Count == 0)
        {
            return new DeleteTokenResponse
            {
                Success = false,
                Message = "Device token not found"
            };
        }

        // Deactivate all matching tokens
        var now = VietnamTime.UtcNow();
        foreach (var token in deviceTokens)
        {
            token.IsActive = false;
            token.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new DeleteTokenResponse
        {
            Success = true,
            Message = $"Device token(s) deactivated successfully. Count: {deviceTokens.Count}"
        };
    }
}

