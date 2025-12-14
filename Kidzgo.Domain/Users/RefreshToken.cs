namespace Kidzgo.Domain.Users;

public class RefreshToken 
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime Expires { get; set; }
    
    //navigation property
    public User User { get; set; } = null!;
}