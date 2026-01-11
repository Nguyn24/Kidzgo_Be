using FluentValidation;

namespace Kidzgo.Application.Exams.UpdateExamResult;

public sealed class UpdateExamResultCommandValidator : AbstractValidator<UpdateExamResultCommand>
{
    public UpdateExamResultCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .WithMessage("Exam Result ID is required");
    }
}

