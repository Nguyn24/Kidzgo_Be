using FluentValidation;

namespace Kidzgo.Application.Exams.UpdateExamQuestion;

public sealed class UpdateExamQuestionCommandValidator : AbstractValidator<UpdateExamQuestionCommand>
{
    public UpdateExamQuestionCommandValidator()
    {
        RuleFor(x => x.QuestionId)
            .NotEmpty()
            .WithMessage("Question ID is required");

        When(x => x.OrderIndex.HasValue, () =>
        {
            RuleFor(x => x.OrderIndex!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Order index must be greater than or equal to 0");
        });

        When(x => !string.IsNullOrWhiteSpace(x.QuestionText), () =>
        {
            RuleFor(x => x.QuestionText!)
                .MaximumLength(2000)
                .WithMessage("Question text must not exceed 2000 characters");
        });

        When(x => x.Points.HasValue, () =>
        {
            RuleFor(x => x.Points!.Value)
                .GreaterThan(0)
                .WithMessage("Points must be greater than 0");
        });
    }
}


