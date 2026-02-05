using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.NotificationTemplates.GetAllNotificationTemplates;

public sealed class GetAllNotificationTemplatesQuery : IQuery<GetAllNotificationTemplatesResponse>, IPageableQuery
{
    public NotificationChannel? Channel { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsDeleted { get; init; } = false;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

