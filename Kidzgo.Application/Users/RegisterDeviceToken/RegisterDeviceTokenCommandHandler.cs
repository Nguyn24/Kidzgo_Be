using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.RegisterDeviceToken;

public sealed class RegisterDeviceTokenCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<RegisterDeviceTokenCommand, RegisterDeviceTokenResponse>
{
    public async Task<Result<RegisterDeviceTokenResponse>> Handle(
        RegisterDeviceTokenCommand command,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var now = VietnamTime.UtcNow();

        // Check if device token already exists for this user and device
        var existingToken = await context.DeviceTokens
            .FirstOrDefaultAsync(
                dt => dt.UserId == userId &&
                      dt.Token == command.Token &&
                      dt.IsActive,
                cancellationToken);

        if (existingToken != null)
        {
            // Update last used time and other metadata
            existingToken.LastUsedAt = now;
            existingToken.UpdatedAt = now;
            if (!string.IsNullOrWhiteSpace(command.Role))
                existingToken.Role = command.Role;
            if (!string.IsNullOrWhiteSpace(command.Browser))
                existingToken.Browser = command.Browser;
            if (!string.IsNullOrWhiteSpace(command.Locale))
                existingToken.Locale = command.Locale;
            if (command.BranchId.HasValue)
                existingToken.BranchId = command.BranchId;
            
            await context.SaveChangesAsync(cancellationToken);

            return new RegisterDeviceTokenResponse
            {
                DeviceTokenId = existingToken.Id,
                Message = "Device token updated successfully"
            };
        }

        // If DeviceId provided, check for existing token with same DeviceId and deactivate old ones
        if (!string.IsNullOrWhiteSpace(command.DeviceId))
        {
            var oldTokens = await context.DeviceTokens
                .Where(dt => dt.UserId == userId &&
                             dt.DeviceId == command.DeviceId &&
                             dt.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var oldToken in oldTokens)
            {
                oldToken.IsActive = false;
                oldToken.UpdatedAt = now;
            }
        }

        // Create new device token
        var deviceToken = new DeviceToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = command.Token,
            DeviceType = command.DeviceType,
            DeviceId = command.DeviceId,
            Role = command.Role,
            Browser = command.Browser,
            Locale = command.Locale,
            BranchId = command.BranchId,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            LastUsedAt = now
        };

        context.DeviceTokens.Add(deviceToken);
        await context.SaveChangesAsync(cancellationToken);

        return new RegisterDeviceTokenResponse
        {
            DeviceTokenId = deviceToken.Id,
            Message = "Device token registered successfully"
        };
    }
}

