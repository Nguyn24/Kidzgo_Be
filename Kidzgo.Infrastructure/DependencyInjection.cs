using System.Text;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Payments;
using Kidzgo.Infrastructure.Authentication;
using Kidzgo.Infrastructure.BackgroundJobs;
using Kidzgo.Infrastructure.Database;
using Kidzgo.Infrastructure.Payments;
using Kidzgo.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Quartz;
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
            .AddPayOSService(configuration)
            .AddZaloService(configuration)
            .AddAuthenticationInternal(configuration)
            .AddBackgroundJobs(configuration);

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var jobKey = new JobKey(nameof(SyncPlannedToActualSessionsJob));

        // Ưu tiên lấy lịch từ cấu hình (appsettings) nếu có
        // Key: "Quartz:Schedules:SyncPlannedToActualSessionsJob"
        var cron = configuration["Quartz:Schedules:SyncPlannedToActualSessionsJob"];

        services.AddQuartz(q =>
        {
            q.AddJob<SyncPlannedToActualSessionsJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(opts =>
            {
                opts.ForJob(jobKey)
                    .WithIdentity($"{nameof(SyncPlannedToActualSessionsJob)}.trigger");

                if (!string.IsNullOrWhiteSpace(cron))
                {
                    opts.WithCronSchedule(cron, x => x.WithMisfireHandlingInstructionDoNothing());
                }
                else
                {
                    // Fallback: mỗi 1 phút
                    opts.WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(1)
                        .RepeatForever());
                }
            });
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default));
            
            // Suppress PendingModelChangesWarning để không bị throw exception khi Migrate()
            // Warning này có thể xuất hiện khi model và snapshot không khớp hoàn toàn
            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

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

    private static IServiceCollection AddPayOSService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PayOSOptions>(configuration.GetSection(PayOSOptions.SectionName));
        
        services.AddHttpClient<IPayOSService, PayOSService>(client =>
        {
            var options = configuration.GetSection(PayOSOptions.SectionName).Get<PayOSOptions>();
            if (options != null)
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }
        });

        return services;
    }

    private static IServiceCollection AddZaloService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<Shared.ZaloSettings>(configuration.GetSection("Zalo"));
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
        services.AddScoped<IMailService, MailService>();
        services.AddSingleton<IPayload, Payload>();
        services.AddScoped<ITemplateRenderer, TemplateRenderer>();
        services.AddScoped<IImageUploader, ImageUploader>();
        services.AddScoped<Application.Abstraction.Storage.IFileStorageService, CloudinaryFileStorageService>();
        services.AddScoped<Kidzgo.Application.Services.SessionGenerationService>();
        return services;
    }

    
}
