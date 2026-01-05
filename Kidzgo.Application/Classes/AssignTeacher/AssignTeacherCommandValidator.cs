using FluentValidation;

namespace Kidzgo.Application.Classes.AssignTeacher;

public sealed class AssignTeacherCommandValidator : AbstractValidator<AssignTeacherCommand>
{
    public AssignTeacherCommandValidator()
    {
        RuleFor(command => command.ClassId)
            .NotEmpty().WithMessage("Class ID is required");

        RuleFor(command => command)
            .Must(c => c.MainTeacherId.HasValue || c.AssistantTeacherId.HasValue)
            .WithMessage("At least one teacher (Main or Assistant) must be assigned");
    }
}

