using Microsoft.AspNetCore.Http;

namespace Kidzgo.API.Requests;

public sealed class UpdateCurrentUserFormRequest
{
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? AvatarUrl { get; init; }
    public Guid? TargetProfileId { get; init; }
    public IFormFile? Avatar { get; init; }
    public List<UpdateCurrentUserProfileFormRequest>? Profiles { get; init; }
}

public sealed class UpdateCurrentUserProfileFormRequest
{
    public Guid Id { get; init; }
    public string? DisplayName { get; init; }
    public bool? IsActive { get; init; }
}
