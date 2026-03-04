using System.Text.Json.Serialization;
using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.ModelBinding;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;


namespace Kidzgo.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
            options.Limits.MaxRequestBufferSize = 100 * 1024 * 1024; // 100 MB
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGenWithAuth();

        services
            .AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new DateOnlyModelBinderProvider());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new NullableDateTimeJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new NullableDateOnlyJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new NullableGuidJsonConverterFactory());
            });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        // Register aggregation service for Monthly Report
        
        
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
