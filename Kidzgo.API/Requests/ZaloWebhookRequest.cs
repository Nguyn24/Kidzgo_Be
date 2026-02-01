namespace Kidzgo.API.Requests;

public sealed class ZaloWebhookRequest
{
    public string Event { get; set; } = null!;
    public string? UserId { get; set; }
    public string? OAId { get; set; }
    public long? Timestamp { get; set; }
    public ZaloMessage? Message { get; set; }
    public Dictionary<string, object>? FormData { get; set; }
}

public sealed class ZaloMessage
{
    public string? Text { get; set; }
    public string? Type { get; set; }
}

