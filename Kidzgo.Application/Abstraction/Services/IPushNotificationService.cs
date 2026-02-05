namespace Kidzgo.Application.Abstraction.Services;

public interface IPushNotificationService
{
    /// <summary>
    /// Send push notification to a device token
    /// </summary>
    /// <param name="deviceToken">Device FCM token or push token</param>
    /// <param name="title">Notification title</param>
    /// <param name="body">Notification body</param>
    /// <param name="data">Additional data payload (optional)</param>
    /// <param name="deeplink">Deeplink to open when notification is tapped (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    Task<bool> SendPushNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        string? deeplink = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send push notification to multiple device tokens
    /// </summary>
    Task<Dictionary<string, bool>> SendPushNotificationsAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        string? deeplink = null,
        CancellationToken cancellationToken = default);
}

