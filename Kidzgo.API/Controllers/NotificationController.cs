using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Notifications.BroadcastNotification;
using Kidzgo.Application.Notifications.GetNotifications;
using Kidzgo.Application.Notifications.MarkNotificationAsRead;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/notifications")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly ISender _mediator;

    public NotificationController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-325-339: Xem danh sách Notifications
    /// </summary>
    /// <param name="profileId">Filter by profile ID</param>
    /// <param name="unreadOnly">Filter unread notifications only</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet]
    [Authorize]
    public async Task<IResult> GetNotifications(
        [FromQuery] Guid? profileId,
        [FromQuery] bool? unreadOnly,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationsQuery
        {
            ProfileId = profileId,
            UnreadOnly = unreadOnly,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-325-339: Admin/Staff broadcast notification
    /// </summary>
    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> BroadcastNotification(
        [FromBody] BroadcastNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new BroadcastNotificationCommand
        {
            Title = request.Title,
            Content = request.Content,
            Deeplink = request.Deeplink,
            Channel = request.Channel,
            Role = request.Role,
            BranchId = request.BranchId,
            ClassId = request.ClassId,
            StudentProfileId = request.StudentProfileId,
            UserIds = request.UserIds,
            ProfileIds = request.ProfileIds
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(br => $"/api/notifications/broadcast/{br.CreatedCount}");
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    /// <param name="id">Notification ID</param>
    [HttpPatch("{id:guid}/read")]
    [Authorize]
    public async Task<IResult> MarkNotificationAsRead(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkNotificationAsReadCommand
        {
            NotificationId = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

