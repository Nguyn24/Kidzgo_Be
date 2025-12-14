using System.Text;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Infrastructure.Authentication;
using Kidzgo.Infrastructure.Database;
using Kidzgo.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using What2Gift.Infrastructure.Shared;

namespace Kidzgo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddClientUrl(configuration)            
            .AddMailService(configuration)
            .AddAuthenticationInternal(configuration);

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default)));

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks().AddNpgSql(configuration.GetConnectionString("Database")!);
        return services;
    }
    
    private static IServiceCollection AddMailService(this IServiceCollection services,
        IConfiguration configuration)
    {
        var mailSettings = configuration.GetSection(nameof(MailSettings)).Get<MailSettings>();

        services.Configure<MailSettings>(options =>
        {
            options.SmtpServer = mailSettings!.SmtpServer;
            options.SmtpPort = mailSettings!.SmtpPort;
            options.SmtpUsername = mailSettings!.SmtpUsername;
            options.SmtpPassword = mailSettings!.SmtpPassword;
        });

        services.AddTransient<IMailService, MailService>();
        return services;
    }

    private static IServiceCollection AddClientUrl(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ClientSettings>(configuration.GetSection(nameof(ClientSettings)));
        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true, 
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)
                    ),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });
    
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IMailService, MailService>();
        services.AddSingleton<IPayload, Payload>();
        services.AddScoped<ITemplateRenderer, TemplateRenderer>();
        services.AddScoped<IImageUploader, ImageUploader>();
        return services;
    }

    
}
