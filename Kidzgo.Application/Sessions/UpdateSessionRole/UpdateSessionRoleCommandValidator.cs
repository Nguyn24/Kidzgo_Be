using FluentValidation;

namespace Kidzgo.Application.Sessions.UpdateSessionRole;

public sealed class UpdateSessionRoleCommandValidator : AbstractValidator<UpdateSessionRoleCommand>
{
    public UpdateSessionRoleCommandValidator()
    {
        RuleFor(command => command.SessionRoleId).NotEmpty();
        RuleFor(command => command.PayableUnitPrice)
            .GreaterThanOrEqualTo(0).When(command => command.PayableUnitPrice.HasValue);
        RuleFor(command => command.PayableAllowance)
            .GreaterThanOrEqualTo(0).When(command => command.PayableAllowance.HasValue);
    }
}