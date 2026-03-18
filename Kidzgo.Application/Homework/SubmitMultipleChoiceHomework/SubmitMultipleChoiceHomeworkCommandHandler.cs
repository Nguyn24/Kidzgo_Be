using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Homework.Shared;
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
            if (!homeworkStudent.Assignment.AllowResubmit)
            {
                return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.SubmissionAlreadySubmitted);
            }
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

        var now = DateTime.UtcNow;

        if (homeworkStudent.Assignment.TimeLimitMinutes.HasValue)
        {
            if (!homeworkStudent.StartedAt.HasValue ||
                (homeworkStudent.Assignment.AllowResubmit && homeworkStudent.Status == HomeworkStatus.Graded))
            {
                homeworkStudent.StartedAt = now;
            }

            var deadline = homeworkStudent.StartedAt.Value.AddMinutes(homeworkStudent.Assignment.TimeLimitMinutes.Value);
            if (now > deadline)
            {
                return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.SubmissionTimeExpired);
            }
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

        var answersByQuestionId = new Dictionary<Guid, Guid?>();
        foreach (var answer in command.Answers)
        {
            answersByQuestionId[answer.QuestionId] = answer.SelectedOptionId;
        }

        // Calculate score and build results
        var totalPoints = questions.Sum(q => q.Points);
        var totalCount = questions.Count;
        var earnedPoints = 0;
        var correctCount = 0;
        var answerResults = new List<QuizAnswerResultDto>();
        var skippedCount = 0;

        foreach (var question in questions)
        {
            // Parse options for display
            var optionTexts = QuizOptionUtils.ParseOptions(question.Options);
            var optionIdByIndex = optionTexts
                .Select((_, idx) => QuizOptionUtils.BuildOptionId(question.Id, idx))
                .ToList();
            var optionTextById = optionIdByIndex
                .Select((id, idx) => new { id, text = optionTexts[idx] })
                .ToDictionary(x => x.id, x => x.text);

            var correctOptionId = (Guid?)null;
            var correctOptionText = (string?)null;
            if (int.TryParse(question.CorrectAnswer, out var correctIdx) &&
                correctIdx >= 0 &&
                correctIdx < optionTexts.Count)
            {
                correctOptionId = optionIdByIndex[correctIdx];
                correctOptionText = optionTexts[correctIdx];
            }

            var selectedOptionId = answersByQuestionId.TryGetValue(question.Id, out var selectedId)
                ? selectedId
                : null;

            var selectedOptionText = selectedOptionId.HasValue
                ? optionTextById.TryGetValue(selectedOptionId.Value, out var text)
                    ? text
                    : null
                : null;

            var isSkipped = !selectedOptionId.HasValue || selectedOptionId == Guid.Empty;
            if (isSkipped)
            {
                skippedCount++;
            }

            var isCorrect = !isSkipped &&
                            correctOptionId.HasValue &&
                            selectedOptionId == correctOptionId;

            var pointsEarned = isCorrect ? question.Points : 0;
            if (isCorrect)
            {
                correctCount++;
                earnedPoints += pointsEarned;
            }

            answerResults.Add(new QuizAnswerResultDto
            {
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                SelectedOptionId = selectedOptionId,
                SelectedOptionText = selectedOptionText,
                CorrectOptionId = correctOptionId,
                CorrectOptionText = correctOptionText,
                IsCorrect = isCorrect,
                EarnedPoints = pointsEarned,
                MaxPoints = question.Points,
                Explanation = question.Explanation
            });
        }

        homeworkStudent.Status = HomeworkStatus.Graded;
        homeworkStudent.SubmittedAt = now;
        homeworkStudent.GradedAt = now;

        // Store answers as JSON in TextAnswer field
        var answersJson = JsonSerializer.Serialize(command.Answers.Select(a => new
        {
            a.QuestionId,
            a.SelectedOptionId
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
            GradedAt = homeworkStudent.GradedAt!.Value,
            MaxScore = maxScore,
            Score = homeworkStudent.Score,
            RewardStars = rewardStars,
            CorrectCount = correctCount,
            WrongCount = totalCount - correctCount - skippedCount,
            SkippedCount = skippedCount,
            TotalCount = totalCount,
            TotalPoints = totalPoints,
            EarnedPoints = earnedPoints,
            AnswerResults = answerResults
        };
    }
}
