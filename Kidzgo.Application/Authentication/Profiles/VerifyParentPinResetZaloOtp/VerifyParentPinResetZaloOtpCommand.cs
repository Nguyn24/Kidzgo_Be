using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Authentication.Profiles.VerifyParentPinResetZaloOtp;

public sealed class VerifyParentPinResetZaloOtpCommand : ICommand<VerifyParentPinResetZaloOtpResponse>
{
    public Guid ChallengeId { get; init; }
    public string Otp { get; init; } = null!;
}
