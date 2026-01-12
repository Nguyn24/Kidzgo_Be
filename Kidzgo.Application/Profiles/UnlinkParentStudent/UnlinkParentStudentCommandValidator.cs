using FluentValidation;

namespace Kidzgo.Application.Profiles.UnlinkParentStudent;

public class UnlinkParentStudentCommandValidator : AbstractValidator<UnlinkParentStudentCommand>
{
    public UnlinkParentStudentCommandValidator()
    {
        RuleFor(command => command.ParentProfileId).NotEmpty();
        RuleFor(command => command.StudentProfileId).NotEmpty();
    }
}

