using FluentValidation;

namespace Kidzgo.Application.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationId)
            .NotEmpty()
            .WithMessage("Notification ID is required");
    }
}

