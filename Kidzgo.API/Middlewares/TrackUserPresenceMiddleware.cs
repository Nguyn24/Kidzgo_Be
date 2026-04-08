using System.Security.Claims;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Users.Shared;
using Kidzgo.Domain.Users;
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
                    var now = VietnamTime.UtcNow();
                    var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, context.RequestAborted);
                    var hasChanges = false;

                    if (user != null &&
                        (!user.LastSeenAt.HasValue ||
                         now - user.LastSeenAt.Value >= UserPresenceHelper.HeartbeatUpdateInterval))
                    {
                        user.LastSeenAt = now;
                        user.UpdatedAt = now;
                        hasChanges = true;
                    }

                    var studentIdRaw = context.User.FindFirstValue("StudentId");
                    if (user != null && Guid.TryParse(studentIdRaw, out var studentProfileId))
                    {
                        var studentProfile = await dbContext.Profiles.FirstOrDefaultAsync(
                            p => p.Id == studentProfileId &&
                                 p.UserId == userId &&
                                 p.ProfileType == ProfileType.Student,
                            context.RequestAborted);

                        if (studentProfile != null &&
                            (!studentProfile.LastSeenAt.HasValue ||
                             now - studentProfile.LastSeenAt.Value >= UserPresenceHelper.HeartbeatUpdateInterval))
                        {
                            studentProfile.LastSeenAt = now;
                            studentProfile.UpdatedAt = now;
                            hasChanges = true;
                        }
                    }

                    if (hasChanges)
                    {
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
