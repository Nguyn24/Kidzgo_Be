namespace Kidzgo.API.Middlewares;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Check if authorization failed (status code 401 or 403)
        // Only write response if it hasn't been written yet
        if (!context.Response.HasStarted)
        {
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                _logger.LogWarning("Authentication failed for path: {Path}", context.Request.Path);
                
                context.Response.ContentType = "application/json";
                var problemDetails = new
                {
                    status = StatusCodes.Status401Unauthorized,
                    type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
                    title = "Unauthorized",
                    detail = "Authentication required to access this resource."
                };

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
            else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                _logger.LogWarning("Authorization failed for path: {Path}", context.Request.Path);
                
                context.Response.ContentType = "application/json";
                var problemDetails = new
                {
                    status = StatusCodes.Status403Forbidden,
                    type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
                    title = "Forbidden",
                    detail = "You do not have permission to access this resource."
                };

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}

