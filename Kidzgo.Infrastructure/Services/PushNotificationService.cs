using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Kidzgo.Application.Abstraction.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Infrastructure.Services;

/// <summary>
/// Push Notification Service using Firebase Admin SDK
/// UC-330: Gửi Notification qua Push real time
/// </summary>
public class PushNotificationService : IPushNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly FirebaseMessaging _messaging;

    public PushNotificationService(
        IConfiguration configuration,
        ILogger<PushNotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Initialize Firebase Admin SDK if not already initialized
        if (FirebaseApp.DefaultInstance == null)
        {
            var serviceAccountPath = _configuration["FCM:ServiceAccountPath"];
            var serviceAccountJson = _configuration["FCM:ServiceAccountJson"];

            if (!string.IsNullOrWhiteSpace(serviceAccountPath) && File.Exists(serviceAccountPath))
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(serviceAccountPath)
                });
            }
            else if (!string.IsNullOrWhiteSpace(serviceAccountJson))
            {
                try
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromJson(serviceAccountJson)
                    });
                    logger.LogInformation("Firebase Admin SDK initialized using JSON string");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to initialize Firebase with JSON string. Check JSON format.");
                    throw;
                }
            }
            else
            {
                logger.LogWarning("Firebase credentials not configured. Push notifications will not work.");
            }
        }

        _messaging = FirebaseApp.DefaultInstance != null 
            ? FirebaseMessaging.DefaultInstance 
            : null;
    }

    public async Task<bool> SendPushNotificationAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        string? deeplink = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_messaging == null)
            {
                _logger.LogWarning("Firebase Messaging is not initialized. Push notification will not be sent.");
                return false;
            }

            // Build data dictionary (including deeplink if provided)
            var messageData = data != null 
                ? data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : new Dictionary<string, string>();

            // Add deeplink to data if provided
            if (!string.IsNullOrWhiteSpace(deeplink) && !messageData.ContainsKey("deeplink"))
            {
                messageData["deeplink"] = deeplink;
            }

            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = messageData,
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ClickAction = !string.IsNullOrWhiteSpace(deeplink) ? deeplink : null
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            var response = await _messaging.SendAsync(message, cancellationToken);
            
            _logger.LogInformation(
                "Push notification sent successfully to device {DeviceToken}. MessageId: {MessageId}",
                deviceToken, response);
            
            return true;
        }
        catch (FirebaseMessagingException ex)
        {
            // Handle specific Firebase errors
            if (ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument ||
                ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
            {
                _logger.LogWarning(
                    "Invalid or unregistered device token {DeviceToken}. Error: {Error}",
                    deviceToken, ex.Message);
                // Token is invalid, should be removed from database
            }
            else if (ex.MessagingErrorCode == MessagingErrorCode.SenderIdMismatch)
            {
                _logger.LogError(
                    "Sender ID mismatch for device {DeviceToken}. Check Firebase configuration.",
                    deviceToken);
            }
            else
            {
                _logger.LogError(ex,
                    "Firebase error sending push notification to device {DeviceToken}. ErrorCode: {ErrorCode}",
                    deviceToken, ex.MessagingErrorCode);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to device {DeviceToken}", deviceToken);
            return false;
        }
    }

    public async Task<Dictionary<string, bool>> SendPushNotificationsAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null,
        string? deeplink = null,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>();

        if (_messaging == null || deviceTokens.Count == 0)
        {
            return results;
        }

        try
        {
            // Prepare data dictionary
            var messageData = data != null 
                ? data.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                : new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(deeplink) && !messageData.ContainsKey("deeplink"))
            {
                messageData["deeplink"] = deeplink;
            }

            // Use batch sending for better performance (FCM supports up to 500 tokens per batch)
            const int batchSize = 500;
            var batches = deviceTokens
                .Select((token, index) => new { token, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.token).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                try
                {
                    // Send batch using SendMulticastAsync (more efficient than individual sends)
                    // FCM supports up to 500 tokens per batch
                    var batchResponse = await _messaging.SendMulticastAsync(
                        new MulticastMessage
                        {
                            Tokens = batch,
                            Notification = new Notification
                            {
                                Title = title,
                                Body = body
                            },
                            Data = messageData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                            Android = new AndroidConfig
                            {
                                Priority = Priority.High,
                                Notification = new AndroidNotification
                                {
                                    Sound = "default",
                                    ClickAction = !string.IsNullOrWhiteSpace(deeplink) ? deeplink : null
                                }
                            },
                            Apns = new ApnsConfig
                            {
                                Aps = new Aps
                                {
                                    Sound = "default",
                                    Badge = 1
                                }
                            }
                        },
                        cancellationToken);

                    // Process results
                    for (int i = 0; i < batch.Count; i++)
                    {
                        var token = batch[i];
                        if (i < batchResponse.Responses.Count)
                        {
                            var response = batchResponse.Responses[i];
                            if (response.IsSuccess)
                            {
                                results[token] = true;
                                _logger.LogDebug(
                                    "Push notification sent successfully to device {DeviceToken}. MessageId: {MessageId}",
                                    token, response.MessageId);
                            }
                            else
                            {
                                results[token] = false;
                                
                                // Check if token is invalid/unregistered
                                if (response.Exception is FirebaseMessagingException fcmEx &&
                                    (fcmEx.MessagingErrorCode == MessagingErrorCode.InvalidArgument ||
                                     fcmEx.MessagingErrorCode == MessagingErrorCode.Unregistered))
                                {
                                    _logger.LogWarning(
                                        "Invalid or unregistered device token {DeviceToken}. Should be removed from database.",
                                        token);
                                }
                                else
                                {
                                    _logger.LogWarning(
                                        "Failed to send push notification to device {DeviceToken}. Error: {Error}",
                                        token, response.Exception?.Message);
                                }
                            }
                        }
                        else
                        {
                            results[token] = false;
                        }
                    }

                    _logger.LogInformation(
                        "Batch push notification: {SuccessCount}/{TotalCount} sent successfully",
                        batchResponse.SuccessCount, batch.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending batch push notifications");
                    // Mark all tokens in batch as failed
                    foreach (var token in batch)
                    {
                        results[token] = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendPushNotificationsAsync");
            // Mark all tokens as failed
            foreach (var token in deviceTokens)
            {
                results[token] = false;
            }
        }

        return results;
    }
}

