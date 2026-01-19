using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.UpdateSessionRole;

public sealed class UpdateSessionRoleCommand : ICommand<UpdateSessionRoleResponse>
{
    public Guid SessionRoleId { get; init; }
    public decimal? PayableUnitPrice { get; init; }
    public decimal? PayableAllowance { get; init; }
}