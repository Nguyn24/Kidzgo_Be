using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Users.RegisterDeviceToken;

public sealed class RegisterDeviceTokenCommand : ICommand<RegisterDeviceTokenResponse>
{
    public string Token { get; init; } = null!;
    public string? DeviceType { get; init; }
    public string? DeviceId { get; init; }
    public string? Role { get; init; } // User role: Parent, Teacher, Staff, Admin
    public string? Browser { get; init; } // Browser name: Chrome, Firefox, Safari, Edge
    public string? Locale { get; init; } // Language code: vi, en
    public Guid? BranchId { get; init; } // Branch ID for multi-branch support
}

