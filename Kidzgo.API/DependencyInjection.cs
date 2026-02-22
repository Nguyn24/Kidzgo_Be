using Kidzgo.API.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;


namespace Kidzgo.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        // Swagger is already added in Program.cs via AddSwaggerGenWithAuth()
        // services.AddSwaggerGen(); // Removed to avoid conflict

        services.AddControllers();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        // Configure authorization to return proper status codes
        services.Configure<AuthorizationOptions>(options =>
        {
            options.FallbackPolicy = null; // Don't require auth by default
        });
        
        // ===== CORS =====
        var clientUrls = configuration
            .GetSection("ClientSettings:ClientUrls")
            .Get<string[]>();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowClients", policy =>
            {
                policy.WithOrigins(clientUrls!)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        return services;
    }
}