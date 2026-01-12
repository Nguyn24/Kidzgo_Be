using FluentValidation;

namespace Kidzgo.Application.Branches.UpdateBranch;

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        
        When(command => !string.IsNullOrWhiteSpace(command.Code), () =>
        {
            RuleFor(command => command.Code).MaximumLength(32);
        });
        
        When(command => !string.IsNullOrWhiteSpace(command.Name), () =>
        {
            RuleFor(command => command.Name).MaximumLength(255);
        });
        
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

