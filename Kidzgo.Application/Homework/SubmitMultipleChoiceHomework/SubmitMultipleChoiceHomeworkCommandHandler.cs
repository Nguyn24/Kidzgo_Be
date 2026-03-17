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

        if (homeworkStudent.Status == HomeworkStatus.Missing)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.SubmissionCannotSubmitMissing);
        }

        if (homeworkStudent.Assignment.SubmissionType != SubmissionType.Quiz)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.CannotSubmitMultipleChoice);
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
            .AsNoTracking()
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

        var answersByQuestionId = new Dictionary<Guid, string?>();
        foreach (var answer in command.Answers)
        {
            answersByQuestionId[answer.QuestionId] = answer.Answer;
        }

        // Calculate score and build results
        var totalPoints = questions.Sum(q => q.Points);
        var totalCount = questions.Count;
        var earnedPoints = 0;
        var correctCount = 0;
        var answerResults = new List<AnswerResultDto>();

        foreach (var question in questions)
        {
            // Parse options for display
            List<string> options = new();
            if (!string.IsNullOrEmpty(question.Options))
            {
                try
                {
                    options = JsonSerializer.Deserialize<List<string>>(question.Options) ?? new List<string>();
                }
                catch
                {
                    // Ignore parse errors
                    options = new List<string>();
                }
            }

            var studentAnswerRaw = answersByQuestionId.TryGetValue(question.Id, out var rawAnswer)
                ? rawAnswer?.Trim() ?? string.Empty
                : string.Empty;

            var correctAnswerRaw = question.CorrectAnswer?.Trim() ?? string.Empty;

            var correctIndex = -1;
            var correctText = correctAnswerRaw;
            if (options.Count > 0 &&
                int.TryParse(correctAnswerRaw, out var correctIdx) &&
                correctIdx >= 0 &&
                correctIdx < options.Count)
            {
                correctIndex = correctIdx;
                correctText = options[correctIdx];
            }

            var studentIndex = -1;
            var studentText = studentAnswerRaw;
            if (options.Count > 0 &&
                int.TryParse(studentAnswerRaw, out var studentIdx) &&
                studentIdx >= 0 &&
                studentIdx < options.Count)
            {
                studentIndex = studentIdx;
                studentText = options[studentIdx];
            }

            bool isCorrect;
            if (question.QuestionType == HomeworkQuestionType.MultipleChoice && options.Count > 0 && correctIndex >= 0)
            {
                isCorrect = studentIndex >= 0
                    ? studentIndex == correctIndex
                    : string.Equals(studentText, correctText, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                isCorrect = string.Equals(studentText, correctText, StringComparison.OrdinalIgnoreCase);
            }

            var pointsEarned = isCorrect ? question.Points : 0;
            if (isCorrect)
            {
                correctCount++;
                earnedPoints += pointsEarned;
            }

            answerResults.Add(new AnswerResultDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                StudentAnswer = studentText,
                CorrectAnswer = correctText,
                IsCorrect = isCorrect,
                Points = pointsEarned,
                Explanation = question.Explanation
            });
        }

        var now = DateTime.UtcNow;
        homeworkStudent.Status = HomeworkStatus.Graded;
        homeworkStudent.SubmittedAt = now;
        homeworkStudent.GradedAt = now;

        // Store answers as JSON in TextAnswer field
        var answersJson = JsonSerializer.Serialize(command.Answers.Select(a => new
        {
            a.QuestionId,
            a.Answer
        }).ToList());
        homeworkStudent.TextAnswer = answersJson;

        // Calculate and store score
        var maxScore = homeworkStudent.Assignment.MaxScore ?? totalPoints;
        homeworkStudent.Score = totalPoints > 0
            ? (decimal)earnedPoints / totalPoints * maxScore
            : 0;

        await context.SaveChangesAsync(cancellationToken);

        // Award stars for on-time submission
        var rewardStars = 0;
        var isOnTime = !homeworkStudent.Assignment.DueAt.HasValue || now <= homeworkStudent.Assignment.DueAt.Value;
        if (isOnTime &&
            homeworkStudent.Assignment.RewardStars.HasValue &&
            homeworkStudent.Assignment.RewardStars.Value > 0)
        {
            rewardStars = homeworkStudent.Assignment.RewardStars.Value;
            await gamificationService.AddStarsForHomeworkCompletion(
                homeworkStudent.StudentProfileId,
                rewardStars,
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
            MaxScore = homeworkStudent.Assignment.MaxScore,
            Score = homeworkStudent.Score,
            RewardStars = rewardStars,
            CorrectCount = correctCount,
            TotalCount = totalCount,
            TotalPoints = totalPoints,
            EarnedPoints = earnedPoints,
            AnswerResults = answerResults
        };
    }
}

