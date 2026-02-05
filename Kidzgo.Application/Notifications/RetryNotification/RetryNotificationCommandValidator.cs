using FluentValidation;

namespace Kidzgo.Application.Notifications.RetryNotification;

public sealed class RetryNotificationCommandValidator : AbstractValidator<RetryNotificationCommand>
{
    public RetryNotificationCommandValidator()
    {
        RuleFor(command => command.NotificationId)
            .NotEmpty().WithMessage("Notification ID is required");
    }
}

