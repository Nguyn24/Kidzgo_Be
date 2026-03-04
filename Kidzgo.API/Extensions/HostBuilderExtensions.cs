namespace Kidzgo.API.Extensions;

public static class HostBuilderExtensions
{
    public static WebApplicationBuilder ConfigurePresentationHost(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables();

        // Configure Kestrel server limits for long-running requests (e.g., Monthly Report aggregation)
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
        });

        return builder;
    }
}
