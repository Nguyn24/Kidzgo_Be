using FluentValidation;

namespace Kidzgo.Application.Blogs.UnpublishBlog;

public sealed class UnpublishBlogCommandValidator : AbstractValidator<UnpublishBlogCommand>
{
    public UnpublishBlogCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Blog ID is required");
    }
}

