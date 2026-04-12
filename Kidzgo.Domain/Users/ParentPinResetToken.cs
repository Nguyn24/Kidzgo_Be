namespace Kidzgo.Domain.Users;

public class ParentPinResetToken
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public string Token { get; set; } = null!;
    public string? OtpCodeHash { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public DateTime? OtpVerifiedAt { get; set; }
    public int OtpAttemptCount { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public bool IsUsed => UsedAt is not null || VietnamTime.UtcNow() > ExpiresAt;
    public bool RequiresOtpVerification => !string.IsNullOrWhiteSpace(OtpCodeHash);

    public Profile Profile { get; set; } = null!;
}

