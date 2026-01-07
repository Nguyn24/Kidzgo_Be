namespace Kidzgo.Application.Sessions.GetRoles;

public sealed class GetRolesResponse
{
    public List<RoleDto> Roles { get; init; } = new();
}

public sealed class RoleDto
{
    public string Value { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
}

