namespace Kidzgo.Domain.Users;

public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public bool IsUsed => UsedAt is not null || DateTime.UtcNow > ExpiresAt;

    public User User { get; set; } = null!;
}


