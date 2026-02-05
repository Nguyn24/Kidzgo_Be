using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.RegisterDeviceToken;

public sealed class RegisterDeviceTokenCommand : ICommand<RegisterDeviceTokenResponse>
{
    public string Token { get; init; } = null!;
    public string? DeviceType { get; init; }
    public string? DeviceId { get; init; }
}

