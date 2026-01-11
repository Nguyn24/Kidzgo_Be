using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Notifications.GetNotifications;

public sealed class GetNotificationsQuery : IQuery<GetNotificationsResponse>, IPageableQuery
{
    public Guid? ProfileId { get; init; }
    public bool? UnreadOnly { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

