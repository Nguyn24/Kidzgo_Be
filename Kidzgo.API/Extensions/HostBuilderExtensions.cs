using System.Security;
using Serilog;
using Serilog.Events;

namespace Kidzgo.API.Extensions;

public static class HostBuilderExtensions
{
    public static WebApplicationBuilder ConfigurePresentationHost(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables();

        ConfigureSerilog(builder);

        // Configure Kestrel server limits for long-running requests (e.g., Monthly Report aggregation)
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
        });

        return builder;
    }

    private static void ConfigureSerilog(WebApplicationBuilder builder)
    {
        var logPath = builder.Configuration["Serilog:File:Path"];
        if (string.IsNullOrWhiteSpace(logPath))
        {
            logPath = Path.Combine(AppContext.BaseDirectory, "logs", "kidzgo-.log");
        }

        var logDirectory = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrWhiteSpace(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "Kidzgo.API")
                .WriteTo.Console()
                .WriteTo.File(
                    path: logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: context.Configuration.GetValue<int?>("Serilog:File:RetainedFileCountLimit") ?? 14,
                    fileSizeLimitBytes: context.Configuration.GetValue<long?>("Serilog:File:FileSizeLimitBytes") ?? 20_971_520,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    restrictedToMinimumLevel: ParseLevel(
                        context.Configuration["Serilog:File:RestrictedToMinimumLevel"],
                        LogEventLevel.Information));

            var seqServerUrl = context.Configuration["Serilog:Seq:ServerUrl"];
            if (!string.IsNullOrWhiteSpace(seqServerUrl))
            {
                loggerConfiguration.WriteTo.Seq(
                    serverUrl: seqServerUrl,
                    restrictedToMinimumLevel: ParseLevel(
                        context.Configuration["Serilog:Seq:RestrictedToMinimumLevel"],
                        LogEventLevel.Information));
            }

            if (OperatingSystem.IsWindows())
            {
                TryConfigureWindowsEventLog(context, loggerConfiguration);
            }
        });
    }

    private static LogEventLevel ParseLevel(string? value, LogEventLevel fallback)
    {
        return Enum.TryParse<LogEventLevel>(value, ignoreCase: true, out var parsed)
            ? parsed
            : fallback;
    }

    private static void TryConfigureWindowsEventLog(
        HostBuilderContext context,
        LoggerConfiguration loggerConfiguration)
    {
        var enabled = context.Configuration.GetValue<bool?>("Serilog:EventLog:Enabled") ?? true;
        if (!enabled)
        {
            return;
        }

        try
        {
            loggerConfiguration.WriteTo.EventLog(
                source: context.Configuration["Serilog:EventLog:Source"] ?? "Kidzgo.API",
                manageEventSource: true,
                restrictedToMinimumLevel: ParseLevel(
                    context.Configuration["Serilog:EventLog:RestrictedToMinimumLevel"],
                    LogEventLevel.Error));
        }
        catch (SecurityException ex)
        {
            Console.Error.WriteLine(
                $"[Serilog] EventLog sink disabled because permissions are insufficient: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine(
                $"[Serilog] EventLog sink disabled because access was denied: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"[Serilog] EventLog sink disabled because initialization failed: {ex.Message}");
        }
    }
}
