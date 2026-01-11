using FluentValidation;

namespace Kidzgo.Application.Notifications.BroadcastNotification;

public sealed class BroadcastNotificationCommandValidator : AbstractValidator<BroadcastNotificationCommand>
{
    public BroadcastNotificationCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Content)
            .MaximumLength(1000)
            .WithMessage("Content must not exceed 1000 characters");

        RuleFor(x => x)
            .Must(HaveAtLeastOneFilter)
            .WithMessage("At least one filter (Role, BranchId, ClassId, StudentProfileId, UserIds, or ProfileIds) must be specified");
    }

    private bool HaveAtLeastOneFilter(BroadcastNotificationCommand command)
    {
        // At least one filter must be specified
        return !string.IsNullOrWhiteSpace(command.Role) ||
               command.BranchId.HasValue ||
               command.ClassId.HasValue ||
               command.StudentProfileId.HasValue ||
               (command.UserIds != null && command.UserIds.Any()) ||
               (command.ProfileIds != null && command.ProfileIds.Any());
    }
}

