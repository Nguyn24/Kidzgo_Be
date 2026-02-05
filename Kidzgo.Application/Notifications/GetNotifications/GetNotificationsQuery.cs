using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.Notifications.GetNotifications;

public sealed class GetNotificationsQuery : IQuery<GetNotificationsResponse>, IPageableQuery
{
    public Guid? ProfileId { get; init; }
    public bool? UnreadOnly { get; init; }
    public NotificationStatus? Status { get; init; } // UC-338: Filter by status (PENDING/SENT/FAILED)
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

