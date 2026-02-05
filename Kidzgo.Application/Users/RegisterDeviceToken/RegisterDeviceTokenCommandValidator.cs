using FluentValidation;

namespace Kidzgo.Application.Users.RegisterDeviceToken;

public sealed class RegisterDeviceTokenCommandValidator : AbstractValidator<RegisterDeviceTokenCommand>
{
    public RegisterDeviceTokenCommandValidator()
    {
        RuleFor(command => command.Token)
            .NotEmpty().WithMessage("Device token is required")
            .MaximumLength(500).WithMessage("Device token must not exceed 500 characters");

        RuleFor(command => command.DeviceType)
            .MaximumLength(50).WithMessage("Device type must not exceed 50 characters")
            .When(command => !string.IsNullOrEmpty(command.DeviceType));

        RuleFor(command => command.DeviceId)
            .MaximumLength(200).WithMessage("Device ID must not exceed 200 characters")
            .When(command => !string.IsNullOrEmpty(command.DeviceId));
    }
}

