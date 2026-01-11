using FluentValidation;
using Kidzgo.Domain.Exams;

namespace Kidzgo.Application.Exams.CreateExamQuestion;

public sealed class CreateExamQuestionCommandValidator : AbstractValidator<CreateExamQuestionCommand>
{
    public CreateExamQuestionCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty()
            .WithMessage("Exam ID is required");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Order index must be greater than or equal to 0");

        RuleFor(x => x.QuestionText)
            .NotEmpty()
            .WithMessage("Question text is required")
            .MaximumLength(2000)
            .WithMessage("Question text must not exceed 2000 characters");

        RuleFor(x => x.QuestionType)
            .IsInEnum()
            .WithMessage("Invalid question type");

        RuleFor(x => x.Points)
            .GreaterThan(0)
            .WithMessage("Points must be greater than 0");

        RuleFor(x => x)
            .Must(HaveValidOptionsForMultipleChoice)
            .WithMessage("Options are required for MultipleChoice questions");

        RuleFor(x => x)
            .Must(HaveCorrectAnswerForMultipleChoice)
            .WithMessage("Correct answer is required for MultipleChoice questions");
    }

    private bool HaveValidOptionsForMultipleChoice(CreateExamQuestionCommand command)
    {
        if (command.QuestionType == QuestionType.MultipleChoice)
        {
            return !string.IsNullOrWhiteSpace(command.Options);
        }
        return true;
    }

    private bool HaveCorrectAnswerForMultipleChoice(CreateExamQuestionCommand command)
    {
        if (command.QuestionType == QuestionType.MultipleChoice)
        {
            return !string.IsNullOrWhiteSpace(command.CorrectAnswer);
        }
        return true;
    }
}


