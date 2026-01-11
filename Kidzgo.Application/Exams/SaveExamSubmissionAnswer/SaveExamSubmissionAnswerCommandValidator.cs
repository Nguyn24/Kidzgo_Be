using FluentValidation;

namespace Kidzgo.Application.Exams.SaveExamSubmissionAnswer;

public sealed class SaveExamSubmissionAnswerCommandValidator : AbstractValidator<SaveExamSubmissionAnswerCommand>
{
    public SaveExamSubmissionAnswerCommandValidator()
    {
        RuleFor(x => x.SubmissionId)
            .NotEmpty()
            .WithMessage("Submission ID is required");

        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage("Question ID is required");

        RuleFor(x => x.Answer)
            .NotEmpty()
            .WithMessage("Answer is required");
    }
}


