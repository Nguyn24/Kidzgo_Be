namespace What2Gift.Infrastructure.Shared;

public class ClientSettings
{
    public string? FrontendUrl { get; set; }
    public string? ApiUrl { get; set; }
    public string[] ClientUrls { get; set; } = [];
}
