using FluentValidation;

namespace Kidzgo.Application.Homework.UpdateHomeworkAssignment;

public sealed class UpdateHomeworkAssignmentCommandValidator : AbstractValidator<UpdateHomeworkAssignmentCommand>
{
    public UpdateHomeworkAssignmentCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Homework Assignment ID is required");

        RuleFor(command => command.DueAt)
            .GreaterThanOrEqualTo(VietnamTime.UtcNow())
            .WithMessage("Due date should not be in the past")
            .When(command => command.DueAt.HasValue);
    }
}

