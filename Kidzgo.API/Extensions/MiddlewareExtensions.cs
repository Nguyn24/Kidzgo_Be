using Kidzgo.API.Middlewares;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;

namespace Kidzgo.API.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();
        return app;
    }

    public static WebApplication UsePresentation(this WebApplication app)
    {
        app.UseSwaggerWithUi();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseCors("AllowClients");
        app.UseRequestContextLogging();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAuthStatusCodePages();

        app.UseMiddleware<AuthorizationMiddleware>();
        app.ApplyMigrations();

        var storageBasePath = app.Configuration["FileStorage:Local:BasePath"] ?? "/var/www/kidzgo/storage";
        if (!Directory.Exists(storageBasePath))
        {
            Directory.CreateDirectory(storageBasePath);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storageBasePath),
            RequestPath = "/storage"
        });

        app.MapControllers();
        return app;
    }

    private static IApplicationBuilder UseAuthStatusCodePages(this IApplicationBuilder app)
    {
        app.UseStatusCodePages(async context =>
        {
            if (context.HttpContext.Response.StatusCode == StatusCodes.Status401Unauthorized ||
                context.HttpContext.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                context.HttpContext.Response.ContentType = "application/json";

                var problemDetails = context.HttpContext.Response.StatusCode == StatusCodes.Status401Unauthorized
                    ? new
                    {
                        status = StatusCodes.Status401Unauthorized,
                        type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1",
                        title = "Unauthorized",
                        detail = "Authentication required to access this resource."
                    }
                    : new
                    {
                        status = StatusCodes.Status403Forbidden,
                        type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3",
                        title = "Forbidden",
                        detail = "You do not have permission to access this resource."
                    };

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails);
            }
        });

        return app;
    }
}
