using Kidzgo.API.Infrastructure;

namespace Kidzgo.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddControllers();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalAndProdFE", policy =>
                policy.WithOrigins("http://localhost:5173", "")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowedToAllowWildcardSubdomains());
        });
        return services;
    }
}