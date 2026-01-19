using FluentValidation;

namespace Kidzgo.Application.Sessions.CreateSessionRole;

public sealed class CreateSessionRoleCommandValidator : AbstractValidator<CreateSessionRoleCommand>
{
    public CreateSessionRoleCommandValidator()
    {
        RuleFor(command => command.SessionId).NotEmpty();
        RuleFor(command => command.StaffUserId).NotEmpty();
        RuleFor(command => command.RoleType).IsInEnum();
        RuleFor(command => command.PayableUnitPrice)
            .GreaterThanOrEqualTo(0).When(command => command.PayableUnitPrice.HasValue);
        RuleFor(command => command.PayableAllowance)
            .GreaterThanOrEqualTo(0).When(command => command.PayableAllowance.HasValue);
    }
}