using FluentValidation;

namespace Kidzgo.Application.Exams.GradeExamSubmission;

public sealed class GradeExamSubmissionCommandValidator : AbstractValidator<GradeExamSubmissionCommand>
{
    public GradeExamSubmissionCommandValidator()
    {
        RuleFor(x => x.SubmissionId)
            .NotEmpty()
            .WithMessage("Submission ID is required");

        RuleFor(x => x)
            .Must(HaveFinalScoreOrAnswerGrades)
            .WithMessage("Either FinalScore or AnswerGrades must be provided");

        When(x => x.FinalScore.HasValue, () =>
        {
            RuleFor(x => x.FinalScore!.Value)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Final score must be greater than or equal to 0");
        });

        When(x => x.AnswerGrades != null && x.AnswerGrades.Any(), () =>
        {
            RuleForEach(x => x.AnswerGrades)
                .ChildRules(answerGrade =>
                {
                    answerGrade.RuleFor(x => x.QuestionId)
                        .NotEmpty()
                        .WithMessage("Question ID is required");

                    answerGrade.When(x => x.PointsAwarded.HasValue, () =>
                    {
                        answerGrade.RuleFor(x => x.PointsAwarded!.Value)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage("Points awarded must be greater than or equal to 0");
                    });
                });
        });
    }

    private bool HaveFinalScoreOrAnswerGrades(GradeExamSubmissionCommand command)
    {
        return command.FinalScore.HasValue || 
               (command.AnswerGrades != null && command.AnswerGrades.Any());
    }
}


