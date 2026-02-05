namespace Kidzgo.Application.Users.RegisterDeviceToken;

public sealed class RegisterDeviceTokenResponse
{
    public Guid DeviceTokenId { get; init; }
    public string Message { get; init; } = null!;
}

