namespace Kidzgo.API.Requests;

public sealed class VerifyParentPinResetZaloOtpRequest
{
    public Guid ChallengeId { get; set; }
    public string Otp { get; set; } = null!;
}
