using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.DeleteDeviceToken;

public sealed class DeleteDeviceTokenCommand : ICommand<DeleteTokenResponse>
{
    public string? Token { get; init; }
    public Guid? DeviceId { get; init; }
}
