namespace Kidzgo.API.Requests;

public sealed class RegisterDeviceTokenRequest
{
    public string Token { get; set; } = null!;
    public string? DeviceType { get; set; } // iOS, Android, Web
    public string? DeviceId { get; set; } // Unique device identifier
    public string? Role { get; set; } // User role: Parent, Teacher, Staff, Admin
    public string? Browser { get; set; } // Browser name: Chrome, Firefox, Safari, Edge
    public string? Locale { get; set; } // Language code: vi, en
    public Guid? BranchId { get; set; } // Branch ID for multi-branch support
}

