namespace Kidzgo.Domain.Users;

public class ParentPinResetToken
{
    public Guid Id { get; set; }
    public Guid ProfileId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public bool IsUsed => UsedAt is not null || DateTime.UtcNow > ExpiresAt;

    public Profile Profile { get; set; } = null!;
}

