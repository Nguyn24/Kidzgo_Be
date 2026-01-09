using FluentValidation;

namespace Kidzgo.Application.Media.CreateMedia;

public sealed class CreateMediaCommandValidator : AbstractValidator<CreateMediaCommand>
{
    public CreateMediaCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("BranchId is required");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Url is required")
            .MaximumLength(500).WithMessage("Url must not exceed 500 characters");

        RuleFor(x => x.Caption)
            .MaximumLength(1000).WithMessage("Caption must not exceed 1000 characters");

        RuleFor(x => x.MonthTag)
            .Matches(@"^\d{4}-\d{2}$").WithMessage("MonthTag must be in YYYY-MM format")
            .When(x => !string.IsNullOrEmpty(x.MonthTag));

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Type must be a valid MediaType");

        RuleFor(x => x.ContentType)
            .IsInEnum().WithMessage("ContentType must be a valid MediaContentType");

        RuleFor(x => x.Visibility)
            .IsInEnum().WithMessage("Visibility must be a valid Visibility type");
    }
}

