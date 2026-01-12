using FluentValidation;

namespace Kidzgo.Application.Profiles.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        
        When(command => !string.IsNullOrWhiteSpace(command.DisplayName), () =>
        {
            RuleFor(command => command.DisplayName)
                .MaximumLength(255)
                .WithMessage("Display name must not exceed 255 characters");
        });
    }
}

