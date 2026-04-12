namespace Kidzgo.Application.Authentication.Profiles.RequestParentPinResetZaloOtp;

public sealed class RequestParentPinResetZaloOtpResponse
{
    public Guid ChallengeId { get; init; }
    public DateTime OtpExpiresAt { get; init; }
}
