using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Payroll;

namespace Kidzgo.Application.Sessions.CreateSessionRole;

public sealed class CreateSessionRoleCommand : ICommand<CreateSessionRoleResponse>
{
    public Guid SessionId { get; init; }
    public Guid StaffUserId { get; init; }
    public SessionRoleType RoleType { get; init; }
    public decimal? PayableUnitPrice { get; init; }
    public decimal? PayableAllowance { get; init; }
}