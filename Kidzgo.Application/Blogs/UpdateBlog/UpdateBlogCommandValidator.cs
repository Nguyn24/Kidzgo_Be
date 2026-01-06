using FluentValidation;

namespace Kidzgo.Application.Blogs.UpdateBlog;

public sealed class UpdateBlogCommandValidator : AbstractValidator<UpdateBlogCommand>
{
    public UpdateBlogCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Blog ID is required");

        RuleFor(command => command.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(command => command.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(50000).WithMessage("Content must not exceed 50000 characters");

        RuleFor(command => command.Summary)
            .MaximumLength(500).WithMessage("Summary must not exceed 500 characters")
            .When(command => !string.IsNullOrEmpty(command.Summary));
    }
}

