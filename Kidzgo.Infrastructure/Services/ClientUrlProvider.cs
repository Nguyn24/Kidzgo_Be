using Kidzgo.Application.Abstraction.Services;
using Microsoft.Extensions.Options;
using What2Gift.Infrastructure.Shared;

namespace Kidzgo.Infrastructure.Services;

public sealed class ClientUrlProvider(IOptions<ClientSettings> clientSettingsOptions) : IClientUrlProvider
{
    private const string FallbackFrontendUrl = "https://kidzgo-centre-pvjj.vercel.app/vi";
    private const string FallbackApiUrl = "https://rexengswagger.duckdns.org";

    public string GetFrontendUrl()
    {
        return string.IsNullOrWhiteSpace(clientSettingsOptions.Value.FrontendUrl)
            ? FallbackFrontendUrl
            : clientSettingsOptions.Value.FrontendUrl.TrimEnd('/');
    }

    public string GetApiUrl()
    {
        return string.IsNullOrWhiteSpace(clientSettingsOptions.Value.ApiUrl)
            ? FallbackApiUrl
            : clientSettingsOptions.Value.ApiUrl.TrimEnd('/');
    }
}
