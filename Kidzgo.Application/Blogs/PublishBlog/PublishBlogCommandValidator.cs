using FluentValidation;

namespace Kidzgo.Application.Blogs.PublishBlog;

public sealed class PublishBlogCommandValidator : AbstractValidator<PublishBlogCommand>
{
    public PublishBlogCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Blog ID is required");
    }
}

