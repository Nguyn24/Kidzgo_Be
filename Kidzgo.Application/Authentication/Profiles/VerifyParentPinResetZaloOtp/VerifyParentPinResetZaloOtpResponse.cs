namespace Kidzgo.Application.Authentication.Profiles.VerifyParentPinResetZaloOtp;

public sealed class VerifyParentPinResetZaloOtpResponse
{
    public string ResetToken { get; init; } = null!;
    public DateTime ExpiresAt { get; init; }
}
