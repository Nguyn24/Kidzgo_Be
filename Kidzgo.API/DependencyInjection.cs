using Kidzgo.API.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Kidzgo.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
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
        services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalAndProdFE", policy =>
                policy.WithOrigins(
                        "http://localhost:3000",
                        "https://kidzgo-centre-pvjj.vercel.app"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });
        return services;
    }
}