using FluentValidation;

namespace Kidzgo.Application.NotificationTemplates.DeleteNotificationTemplate;

public sealed class DeleteNotificationTemplateCommandValidator : AbstractValidator<DeleteNotificationTemplateCommand>
{
    public DeleteNotificationTemplateCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}

