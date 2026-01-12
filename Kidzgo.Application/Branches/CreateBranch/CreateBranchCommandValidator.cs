using FluentValidation;

namespace Kidzgo.Application.Branches.CreateBranch;

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(command => command.Code)
            .NotNull()
            .NotEmpty()
            .MaximumLength(32)
            .WithMessage("Branch code is required and must not exceed 32 characters");
        
        RuleFor(command => command.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(255)
            .WithMessage("Branch name is required and must not exceed 255 characters");
        
        When(command => !string.IsNullOrWhiteSpace(command.ContactEmail), () =>
        {
            RuleFor(command => command.ContactEmail).EmailAddress();
        });
        
        When(command => !string.IsNullOrWhiteSpace(command.ContactPhone), () =>
        {
            RuleFor(command => command.ContactPhone).MaximumLength(32);
        });
    }
}

