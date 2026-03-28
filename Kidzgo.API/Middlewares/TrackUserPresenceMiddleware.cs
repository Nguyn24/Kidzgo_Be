using System.Security.Claims;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Users.Shared;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.API.Middlewares;

public sealed class TrackUserPresenceMiddleware(RequestDelegate next, ILogger<TrackUserPresenceMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, IDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var userIdRaw = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdRaw, out var userId))
                {
                    var now = DateTime.UtcNow;
                    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, context.RequestAborted);

                    if (user != null &&
                        (!user.LastSeenAt.HasValue ||
                         now - user.LastSeenAt.Value >= UserPresenceHelper.HeartbeatUpdateInterval))
                    {
                        user.LastSeenAt = now;
                        user.UpdatedAt = now;
                        await dbContext.SaveChangesAsync(context.RequestAborted);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to update user presence");
            }
        }

        await next(context);
    }
}
