using System.Text.Json.Serialization;
using HealthChecks.UI.Client;
using Kidzgo.API.Extensions;
using Kidzgo.Application;
using Kidzgo.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Kidzgo.API;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        
        builder.Configuration.AddEnvironmentVariables();
        
        builder.Services.AddSwaggerGenWithAuth(); 
        
        builder.Services
            .AddApplication()
            .AddPresentation()
            .AddInfrastructure(builder.Configuration);
        
        builder.Services
            .AddControllers(options =>
            {
                options.ModelBinderProviders.Insert(0, new ModelBinding.DateOnlyModelBinderProvider());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new Extensions.DateTimeJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new Extensions.NullableDateTimeJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new Extensions.DateOnlyJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new Extensions.NullableDateOnlyJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new Extensions.NullableGuidJsonConverterFactory());
            });
        
        var app = builder.Build();

        app.UseSwaggerWithUi();  

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseCors("AllowLocalAndProdFE");
        app.UseRequestContextLogging();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        // Handle authorization status codes (401, 403) with custom response
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
        
        app.UseMiddleware<Middlewares.AuthorizationMiddleware>();
        app.ApplyMigrations();  // chạy EF Core migration khi khởi động
        app.MapControllers();

        app.Run();
    }
}