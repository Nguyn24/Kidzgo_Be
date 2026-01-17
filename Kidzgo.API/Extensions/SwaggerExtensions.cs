using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Kidzgo.API.Extensions;

public static class SwaggerExtensions
{
    internal static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
    {
        services.AddSwaggerGen(o =>
        {
            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                o.IncludeXmlComments(xmlPath);
            }
            
            o.CustomSchemaIds(id => id.FullName!.Replace('+', '-'));

            // Configure Swagger to use the same JsonSerializerOptions as the API
            o.UseAllOfToExtendReferenceSchemas();
            o.SupportNonNullableReferenceTypes();
            
            // Map DateTime to string with format (for Swagger schema display)
            o.MapType<DateTime>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date-time",
                Example = new Microsoft.OpenApi.Any.OpenApiString("23/11/2025 10:46:41 PM")
            });
            
            o.MapType<DateTime?>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date-time",
                Nullable = true,
                Example = new Microsoft.OpenApi.Any.OpenApiString("23/11/2025 10:46:41 PM")
            });
            
            // Map DateOnly to string with format
            o.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date",
                Example = new Microsoft.OpenApi.Any.OpenApiString("2025-11-23")
            });
            
            o.MapType<DateOnly?>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date",
                Nullable = true,
                Example = new Microsoft.OpenApi.Any.OpenApiString("2025-11-23")
            });
            
            // Configure Swagger to use the same JsonSerializerOptions
            o.UseOneOfForPolymorphism();
            o.SelectDiscriminatorNameUsing(baseType => "$type");

            var securityScheme = new OpenApiSecurityScheme()
            {
                Name = "JWT Authentication",
                Description = "Enter your JWT token in this field",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT"
            };

            o.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    []
                }
            };

            o.AddSecurityRequirement(securityRequirement);

            // Configure Swagger to handle file uploads
            o.MapType<IFormFile>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            });

            // Add parameter filter for examples
            o.ParameterFilter<SwaggerParameterFilter>();
        });

        return services;
    }
    
    public static IApplicationBuilder UseSwaggerWithUi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }
    
}