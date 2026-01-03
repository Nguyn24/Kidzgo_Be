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
        app.ApplyMigrations();  // chạy EF Core migration khi khởi động
        app.MapControllers();

        app.Run();
    }
}