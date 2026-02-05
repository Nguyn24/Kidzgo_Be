using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.NotificationTemplates.GetNotificationTemplateById;

public sealed class GetNotificationTemplateByIdQuery : IQuery<GetNotificationTemplateByIdResponse>
{
    public Guid Id { get; init; }
}

