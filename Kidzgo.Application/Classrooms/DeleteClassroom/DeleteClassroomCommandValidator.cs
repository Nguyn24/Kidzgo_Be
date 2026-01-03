using FluentValidation;

namespace Kidzgo.Application.Classrooms.DeleteClassroom;

public sealed class DeleteClassroomCommandValidator : AbstractValidator<DeleteClassroomCommand>
{
    public DeleteClassroomCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Classroom ID is required");
    }
}

