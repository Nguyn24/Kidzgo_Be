namespace Kidzgo.Application.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadResponse
{
    public List<Guid> ReadIds { get; init; } = new();
    public List<Guid> AlreadyReadIds { get; init; } = new();
    public List<MarkNotificationAsReadError> Errors { get; init; } = new();
}

public sealed class MarkNotificationAsReadError
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Message { get; init; } = null!;
}

