namespace Kidzgo.Infrastructure.Shared;

public class ZaloSettings
{
    public string AppId { get; set; } = null!;
    public string AppSecret { get; set; } = null!;
    public string? OAId { get; set; }
    public string WebhookVerifyToken { get; set; } = null!;
    public string BaseUrl { get; set; } = "https://openapi.zalo.me/v2.0";
}

