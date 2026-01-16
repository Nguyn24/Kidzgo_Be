using Kidzgo.API.Infrastructure;

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
        services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalAndProdFE", policy =>
                policy.WithOrigins(
                        "http://localhost:5173",
                        "https://kidzgo-centre-pvjj.vercel.app"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });
        return services;
    }
}