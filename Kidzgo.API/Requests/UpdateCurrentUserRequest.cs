namespace Kidzgo.API.Requests;

public sealed class UpdateCurrentUserRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public List<UpdateProfileRequest>? Profiles { get; set; }
}

public sealed class UpdateProfileRequest
{
    public Guid Id { get; set; }
    public string? DisplayName { get; set; }
}

