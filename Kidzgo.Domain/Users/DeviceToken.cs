using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users;

public class DeviceToken : Entity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = null!;
    public string? DeviceType { get; set; } // iOS, Android, Web
    public string? DeviceId { get; set; }
    public string? Role { get; set; } // User role: Parent, Teacher, Staff, Admin
    public string? Browser { get; set; } // Browser name: Chrome, Firefox, Safari, Edge
    public string? Locale { get; set; } // Language code: vi, en
    public Guid? BranchId { get; set; } // Branch ID for multi-branch support
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}

