using FluentValidation;

namespace Kidzgo.Application.Authentication.Profiles.VerifyParentPinResetZaloOtp;

public sealed class VerifyParentPinResetZaloOtpCommandValidator : AbstractValidator<VerifyParentPinResetZaloOtpCommand>
{
    public VerifyParentPinResetZaloOtpCommandValidator()
    {
        RuleFor(x => x.ChallengeId)
            .NotEmpty();

        RuleFor(x => x.Otp)
            .NotEmpty()
            .Length(6)
            .Matches("^[0-9]+$");
    }
}
