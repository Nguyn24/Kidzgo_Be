using FluentValidation;

namespace Kidzgo.Application.NotificationTemplates.CreateNotificationTemplate;

public sealed class CreateNotificationTemplateCommandValidator : AbstractValidator<CreateNotificationTemplateCommand>
{
    public CreateNotificationTemplateCommandValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty().WithMessage("Code is required")
            .MaximumLength(100).WithMessage("Code must not exceed 100 characters");

        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters");

        RuleFor(command => command.Channel)
            .IsInEnum().WithMessage("Channel must be a valid NotificationChannel value");
    }
}

