using FluentValidation;

namespace Kidzgo.Application.Profiles.LinkParentStudent;

public class LinkParentStudentCommandValidator : AbstractValidator<LinkParentStudentCommand>
{
    public LinkParentStudentCommandValidator()
    {
        RuleFor(command => command.ParentProfileId).NotEmpty();
        RuleFor(command => command.StudentProfileId).NotEmpty();
    }
}

