using Kidzgo.Application.Abstraction.Data;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.Shared;

internal static class QuizSubmissionReviewBuilder
{
    public static async Task<(List<HomeworkQuestionDto> Questions, List<QuizAnswerResultDto> AnswerResults)> BuildAsync(
        IDbContext context,
        Guid assignmentId,
        string? textAnswer,
        bool includeReview,
        CancellationToken cancellationToken)
    {
        var questions = new List<HomeworkQuestionDto>();
        var answerResults = new List<QuizAnswerResultDto>();

        var homeworkQuestions = await context.HomeworkQuestions
            .Where(q => q.HomeworkAssignmentId == assignmentId)
            .OrderBy(q => q.OrderIndex)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var selectedOptionByQuestionId = QuizOptionUtils.ParseSelectedOptions(textAnswer);

        foreach (var question in homeworkQuestions)
        {
            var optionTexts = QuizOptionUtils.ParseOptions(question.Options);
            var optionDtos = optionTexts
                .Select((text, idx) => new HomeworkOptionDto
                {
                    Id = QuizOptionUtils.BuildOptionId(question.Id, idx),
                    Text = text,
                    OrderIndex = idx
                })
                .ToList();
            var optionTextById = optionDtos.ToDictionary(o => o.Id, o => o.Text);

            questions.Add(new HomeworkQuestionDto
            {
                Id = question.Id,
                OrderIndex = question.OrderIndex,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType.ToString(),
                Options = optionDtos,
                Points = question.Points
            });

            if (!includeReview)
            {
                continue;
            }

            QuizOptionUtils.TryBuildCorrectOption(
                question.Id,
                optionTexts,
                question.CorrectAnswer,
                out var correctOptionId,
                out var correctOptionText);

            var selectedOptionId = selectedOptionByQuestionId.TryGetValue(question.Id, out var selected)
                ? selected
                : null;

            var selectedOptionText = selectedOptionId.HasValue
                ? optionTextById.TryGetValue(selectedOptionId.Value, out var text)
                    ? text
                    : null
                : null;

            var isCorrect = selectedOptionId.HasValue &&
                            correctOptionId.HasValue &&
                            selectedOptionId == correctOptionId;

            answerResults.Add(new QuizAnswerResultDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                SelectedOptionId = selectedOptionId,
                SelectedOptionText = selectedOptionText,
                CorrectOptionId = correctOptionId,
                CorrectOptionText = correctOptionText,
                IsCorrect = isCorrect,
                EarnedPoints = isCorrect ? question.Points : 0,
                MaxPoints = question.Points,
                Explanation = question.Explanation
            });
        }

        return (questions, answerResults);
    }
}
