using FluentValidation;

namespace Kidzgo.Application.Notifications.MarkNotificationAsRead;

public sealed class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator()
    {
        RuleFor(x => x.NotificationIds)
            .NotEmpty()
            .WithMessage("At least one notification ID is required");

        RuleForEach(x => x.NotificationIds)
            .NotEmpty()
            .WithMessage("Notification ID is required");
    }
}

