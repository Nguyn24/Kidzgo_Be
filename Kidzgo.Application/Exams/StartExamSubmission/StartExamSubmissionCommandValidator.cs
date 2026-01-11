using FluentValidation;

namespace Kidzgo.Application.Exams.StartExamSubmission;

public sealed class StartExamSubmissionCommandValidator : AbstractValidator<StartExamSubmissionCommand>
{
    public StartExamSubmissionCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty()
            .WithMessage("Exam ID is required");

        RuleFor(x => x.StudentProfileId)
            .NotEmpty()
            .WithMessage("Student Profile ID is required");
    }
}


