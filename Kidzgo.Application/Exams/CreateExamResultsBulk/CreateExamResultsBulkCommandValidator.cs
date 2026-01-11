using FluentValidation;

namespace Kidzgo.Application.Exams.CreateExamResultsBulk;

public sealed class CreateExamResultsBulkCommandValidator : AbstractValidator<CreateExamResultsBulkCommand>
{
    public CreateExamResultsBulkCommandValidator()
    {
        RuleFor(command => command.ExamId)
            .NotEmpty()
            .WithMessage("Exam ID is required");

        RuleFor(command => command.Results)
            .NotEmpty()
            .WithMessage("At least one exam result is required");

        RuleForEach(command => command.Results)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.StudentProfileId)
                    .NotEmpty()
                    .WithMessage("Student Profile ID is required");
            });
    }
}

