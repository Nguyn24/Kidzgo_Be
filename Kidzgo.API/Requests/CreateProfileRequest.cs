using Kidzgo.Domain.Users;

namespace Kidzgo.API.Requests;

public sealed class CreateProfileRequest
{
    public Guid UserId { get; set; }
    public ProfileType ProfileType { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? PinHash { get; set; }
}

