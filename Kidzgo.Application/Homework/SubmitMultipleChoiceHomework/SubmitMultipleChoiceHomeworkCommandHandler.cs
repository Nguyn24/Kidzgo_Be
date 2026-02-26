using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.SubmitMultipleChoiceHomework;

public sealed class SubmitMultipleChoiceHomeworkCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IGamificationService gamificationService
) : ICommandHandler<SubmitMultipleChoiceHomeworkCommand, SubmitMultipleChoiceHomeworkResponse>
{
    public async Task<Result<SubmitMultipleChoiceHomeworkResponse>> Handle(
        SubmitMultipleChoiceHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(ProfileErrors.StudentNotFound);
        }

        // Get homework submission
        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .FirstOrDefaultAsync(hs => hs.Id == command.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.SubmissionNotFound(command.HomeworkStudentId));
        }

        // Verify ownership
        if (homeworkStudent.StudentProfileId != studentId.Value)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }

        // Check if already submitted
        if (homeworkStudent.Status == HomeworkStatus.Submitted || homeworkStudent.Status == HomeworkStatus.Graded)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.SubmissionAlreadySubmitted);
        }

        // Validate answers
        if (command.Answers == null || command.Answers.Count == 0)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.NoAnswersProvided);
        }

        // Get questions for this homework
        var questions = await context.HomeworkQuestions
            .Where(q => q.HomeworkAssignmentId == homeworkStudent.AssignmentId)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(cancellationToken);

        if (questions.Count == 0)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.NoQuestionsProvided);
        }

        // Build question lookup
        var questionLookup = questions.ToDictionary(q => q.Id);

        // Validate all answers correspond to existing questions
        foreach (var answer in command.Answers)
        {
            if (!questionLookup.ContainsKey(answer.QuestionId))
            {
                return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.QuestionNotFound(answer.QuestionId));
            }
        }

        // Calculate score and build results
        var totalPoints = 0;
        var earnedPoints = 0;
        var answerResults = new List<AnswerResultDto>();

        foreach (var answer in command.Answers)
        {
            if (!questionLookup.TryGetValue(answer.QuestionId, out var question))
            {
                continue;
            }

            var studentAnswer = answer.Answer?.Trim() ?? "";
            var correctAnswer = question.CorrectAnswer?.Trim() ?? "";
            var isCorrect = string.Equals(studentAnswer, correctAnswer, StringComparison.OrdinalIgnoreCase);

            var pointsEarned = isCorrect ? question.Points : 0;

            totalPoints += question.Points;
            earnedPoints += pointsEarned;

            // Parse options for display
            List<string>? options = null;
            if (!string.IsNullOrEmpty(question.Options))
            {
                try
                {
                    options = JsonSerializer.Deserialize<List<string>>(question.Options);
                }
                catch
                {
                    // Ignore parse errors
                }
            }

            answerResults.Add(new AnswerResultDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                StudentAnswer = options != null && int.TryParse(studentAnswer, out var idx) && idx >= 0 && idx < options.Count
                    ? options[idx]
                    : studentAnswer,
                CorrectAnswer = options != null && int.TryParse(correctAnswer, out var correctIdx) && correctIdx >= 0 && correctIdx < options.Count
                    ? options[correctIdx]
                    : correctAnswer,
                IsCorrect = isCorrect,
                Points = pointsEarned,
                Explanation = question.Explanation
            });
        }

        // Update status
        if (homeworkStudent.Status == HomeworkStatus.Missing ||
            (homeworkStudent.Assignment.DueAt.HasValue && DateTime.UtcNow > homeworkStudent.Assignment.DueAt.Value))
        {
            homeworkStudent.Status = HomeworkStatus.Late;
        }
        else
        {
            homeworkStudent.Status = HomeworkStatus.Submitted;
        }

        homeworkStudent.SubmittedAt = DateTime.UtcNow;

        // Store answers as JSON in TextAnswer field
        var answersJson = JsonSerializer.Serialize(command.Answers.Select(a => new
        {
            a.QuestionId,
            a.Answer
        }).ToList());
        homeworkStudent.TextAnswer = answersJson;

        // Calculate and store score
        if (totalPoints > 0)
        {
            homeworkStudent.Score = (decimal)earnedPoints / totalPoints * (homeworkStudent.Assignment.MaxScore ?? 10);
        }

        await context.SaveChangesAsync(cancellationToken);

        // Award stars for on-time submission
        if (homeworkStudent.Status == HomeworkStatus.Submitted &&
            homeworkStudent.Assignment.RewardStars.HasValue &&
            homeworkStudent.Assignment.RewardStars.Value > 0)
        {
            await gamificationService.AddStarsForHomeworkCompletion(
                homeworkStudent.StudentProfileId,
                homeworkStudent.Assignment.RewardStars.Value,
                homeworkStudent.AssignmentId,
                reason: "On-time Homework Submission",
                cancellationToken);
        }

        return new SubmitMultipleChoiceHomeworkResponse
        {
            Id = homeworkStudent.Id,
            AssignmentId = homeworkStudent.AssignmentId,
            Status = homeworkStudent.Status.ToString(),
            SubmittedAt = homeworkStudent.SubmittedAt!.Value,
            Score = homeworkStudent.Score,
            TotalPoints = totalPoints,
            EarnedPoints = earnedPoints,
            AnswerResults = answerResults
        };
    }
}

