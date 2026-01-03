using FluentValidation;

namespace Kidzgo.Application.Classrooms.UpdateClassroom;

public sealed class UpdateClassroomCommandValidator : AbstractValidator<UpdateClassroomCommand>
{
    public UpdateClassroomCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty().WithMessage("Classroom ID is required");

        RuleFor(command => command.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Classroom name is required")
            .MaximumLength(100).WithMessage("Classroom name must not exceed 100 characters");

        RuleFor(command => command.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0");
    }
}

