namespace Kidzgo.API.Requests;

public sealed class CreateSessionRoleRequest
{
    public Guid StaffUserId { get; set; }
    public string RoleType { get; set; } = null!; // "MainTeacher", "Assistant", "Club", "Workshop"
    public decimal? PayableUnitPrice { get; set; }
    public decimal? PayableAllowance { get; set; }
}