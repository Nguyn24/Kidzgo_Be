namespace Kidzgo.Application.Sessions.UpdateSessionRole;

public sealed class UpdateSessionRoleResponse
{
    public Guid Id { get; init; }
    public Guid SessionId { get; init; }
    public Guid StaffUserId { get; init; }
    public string RoleType { get; init; } = null!;
    public decimal? PayableUnitPrice { get; init; }
    public decimal? PayableAllowance { get; init; }
}