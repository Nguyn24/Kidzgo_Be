namespace Kidzgo.API.Requests;

public sealed class RegisterDeviceTokenRequest
{
    public string Token { get; set; } = null!;
    public string? DeviceType { get; set; } // iOS, Android, Web
    public string? DeviceId { get; set; } // Unique device identifier
}

