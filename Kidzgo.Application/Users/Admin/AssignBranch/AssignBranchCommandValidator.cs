using FluentValidation;

namespace Kidzgo.Application.Users.Admin.AssignBranch;

public class AssignBranchCommandValidator : AbstractValidator<AssignBranchCommand>
{
    public AssignBranchCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
    }
}

