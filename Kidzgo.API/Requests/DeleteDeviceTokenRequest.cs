namespace Kidzgo.API.Requests;

public sealed class DeleteDeviceTokenRequest
{
    public string? Token { get; set; } // FCM token to delete
    public Guid? DeviceId { get; set; } // Or delete all tokens for this device
}

