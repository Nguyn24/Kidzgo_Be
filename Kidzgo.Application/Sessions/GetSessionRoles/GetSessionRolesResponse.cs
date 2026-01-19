namespace Kidzgo.Application.Sessions.GetSessionRoles;

public sealed class GetSessionRolesResponse
{
    public List<SessionRoleDto> Roles { get; init; } = new();
}

public sealed class SessionRoleDto
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid StaffUserId { get; init; }
    public string StaffUserName { get; init; } = null!;
    public string RoleType { get; init; } = null!;
    public decimal? PayableUnitPrice { get; init; }
    public decimal? PayableAllowance { get; init; }
}