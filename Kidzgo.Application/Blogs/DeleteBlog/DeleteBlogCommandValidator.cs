using FluentValidation;

namespace Kidzgo.Application.Blogs.DeleteBlog;

public sealed class DeleteBlogCommandValidator : AbstractValidator<DeleteBlogCommand>
{
    public DeleteBlogCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Blog ID is required");
    }
}

