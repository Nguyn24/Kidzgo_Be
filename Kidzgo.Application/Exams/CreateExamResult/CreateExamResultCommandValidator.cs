using FluentValidation;

namespace Kidzgo.Application.Exams.CreateExamResult;

public sealed class CreateExamResultCommandValidator : AbstractValidator<CreateExamResultCommand>
{
    public CreateExamResultCommandValidator()
    {
        RuleFor(command => command.ExamId)
            .NotEmpty()
            .WithMessage("Exam ID is required");

        RuleFor(command => command.StudentProfileId)
            .NotEmpty()
            .WithMessage("Student Profile ID is required");
    }
}

