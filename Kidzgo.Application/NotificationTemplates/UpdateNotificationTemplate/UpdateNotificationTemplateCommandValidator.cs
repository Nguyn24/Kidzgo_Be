using FluentValidation;

namespace Kidzgo.Application.NotificationTemplates.UpdateNotificationTemplate;

public sealed class UpdateNotificationTemplateCommandValidator : AbstractValidator<UpdateNotificationTemplateCommand>
{
    public UpdateNotificationTemplateCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters");

        RuleFor(command => command.Channel)
            .IsInEnum().WithMessage("Channel must be a valid NotificationChannel value");
    }
}

