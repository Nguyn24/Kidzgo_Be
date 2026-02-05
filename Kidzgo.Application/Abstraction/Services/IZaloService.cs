namespace Kidzgo.Application.Abstraction.Services;

/// <summary>
/// Service for sending messages via Zalo Official Account
/// UC-328: Gửi Notification qua Zalo OA
/// </summary>
public interface IZaloService
{
    /// <summary>
    /// Send text message to Zalo user
    /// </summary>
    /// <param name="zaloUserId">Zalo user ID</param>
    /// <param name="message">Message content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    Task<bool> SendMessageAsync(
        string zaloUserId,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send message with deeplink to Zalo user
    /// </summary>
    /// <param name="zaloUserId">Zalo user ID</param>
    /// <param name="message">Message content</param>
    /// <param name="deeplink">Deeplink URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully, false otherwise</returns>
    Task<bool> SendMessageWithDeeplinkAsync(
        string zaloUserId,
        string message,
        string? deeplink = null,
        CancellationToken cancellationToken = default);
}

