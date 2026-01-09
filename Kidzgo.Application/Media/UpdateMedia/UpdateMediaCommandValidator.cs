using FluentValidation;

namespace Kidzgo.Application.Media.UpdateMedia;

public sealed class UpdateMediaCommandValidator : AbstractValidator<UpdateMediaCommand>
{
    public UpdateMediaCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.Caption)
            .MaximumLength(1000).WithMessage("Caption must not exceed 1000 characters")
            .When(x => x.Caption is not null);

        RuleFor(x => x.MonthTag)
            .Matches(@"^\d{4}-\d{2}$").WithMessage("MonthTag must be in YYYY-MM format")
            .When(x => !string.IsNullOrEmpty(x.MonthTag));

        RuleFor(x => x.ContentType)
            .IsInEnum().WithMessage("ContentType must be a valid MediaContentType")
            .When(x => x.ContentType.HasValue);

        RuleFor(x => x.Visibility)
            .IsInEnum().WithMessage("Visibility must be a valid Visibility type")
            .When(x => x.Visibility.HasValue);
    }
}

