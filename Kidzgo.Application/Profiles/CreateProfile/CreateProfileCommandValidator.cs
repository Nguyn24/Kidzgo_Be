using FluentValidation;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Profiles.CreateProfile;

public class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.DisplayName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Display name is required and must not exceed 255 characters");
        RuleFor(command => command.ProfileType)
            .IsInEnum()
            .WithMessage("Profile type must be Parent or Student");
    }
}

