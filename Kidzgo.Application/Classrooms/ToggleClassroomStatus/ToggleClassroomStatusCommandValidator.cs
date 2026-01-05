using FluentValidation;

namespace Kidzgo.Application.Classrooms.ToggleClassroomStatus;

public sealed class ToggleClassroomStatusCommandValidator : AbstractValidator<ToggleClassroomStatusCommand>
{
    public ToggleClassroomStatusCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Classroom ID is required");
    }
}

