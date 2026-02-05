using FluentValidation;

namespace Kidzgo.Application.NotificationTemplates.GetAllNotificationTemplates;

public sealed class GetAllNotificationTemplatesQueryValidator : AbstractValidator<GetAllNotificationTemplatesQuery>
{
    public GetAllNotificationTemplatesQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(query => query.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");
    }
}

