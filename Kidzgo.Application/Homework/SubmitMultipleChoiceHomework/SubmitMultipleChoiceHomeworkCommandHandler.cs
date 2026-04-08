using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
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

        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .FirstOrDefaultAsync(hs => hs.Id == command.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.SubmissionNotFound(command.HomeworkStudentId));
        }

        if (homeworkStudent.StudentProfileId != studentId.Value)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }

        var persistedAttemptCount = await context.HomeworkSubmissionAttempts
            .CountAsync(a => a.HomeworkStudentId == homeworkStudent.Id, cancellationToken);

        if (persistedAttemptCount == 0 &&
            HomeworkSubmissionAttemptMapper.HasLegacyAttempt(homeworkStudent))
        {
            context.HomeworkSubmissionAttempts.Add(
                HomeworkSubmissionAttemptMapper.BuildLegacyAttempt(
                    homeworkStudent,
                    attemptNumber: 1,
                    id: Guid.NewGuid()));
            persistedAttemptCount = 1;
        }

        var currentAttemptCount = persistedAttemptCount;
        var maxAttempts = homeworkStudent.Assignment.MaxAttempts;
        if (currentAttemptCount >= maxAttempts)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.SubmissionAttemptLimitReached(maxAttempts));
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

        var now = VietnamTime.UtcNow();
        var isFirstSubmission = currentAttemptCount == 0;

        if (homeworkStudent.Assignment.TimeLimitMinutes.HasValue)
        {
            if (!homeworkStudent.StartedAt.HasValue || currentAttemptCount > 0)
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

        if (command.Answers == null || command.Answers.Count == 0)
        {
            return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                HomeworkErrors.NoAnswersProvided);
        }

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

        var questionLookup = questions.ToDictionary(q => q.Id);
        foreach (var answer in command.Answers)
        {
            if (!questionLookup.ContainsKey(answer.QuestionId))
            {
                return Result.Failure<SubmitMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.QuestionNotFound(answer.QuestionId));
            }
        }

        var answersByQuestionId = command.Answers
            .ToDictionary(answer => answer.QuestionId, answer => answer.SelectedOptionId);

        var totalPoints = questions.Sum(q => q.Points);
        var totalCount = questions.Count;
        var earnedPoints = 0;
        var correctCount = 0;
        var skippedCount = 0;
        var answerResults = new List<QuizAnswerResultDto>();

        foreach (var question in questions)
        {
            var optionTexts = QuizOptionUtils.ParseOptions(question.Options);
            var optionIdByIndex = optionTexts
                .Select((_, idx) => QuizOptionUtils.BuildOptionId(question.Id, idx))
                .ToList();
            var optionTextById = optionIdByIndex
                .Select((id, idx) => new { id, text = optionTexts[idx] })
                .ToDictionary(x => x.id, x => x.text);

            QuizOptionUtils.TryBuildCorrectOption(
                question.Id,
                optionTexts,
                question.CorrectAnswer,
                out var correctOptionId,
                out var correctOptionText);

            var selectedOptionId = answersByQuestionId.TryGetValue(question.Id, out var selectedId)
                ? selectedId
                : null;

            var selectedOptionText = selectedOptionId.HasValue &&
                                     optionTextById.TryGetValue(selectedOptionId.Value, out var text)
                ? text
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
        homeworkStudent.TeacherFeedback = null;
        homeworkStudent.AiFeedback = null;

        var answersJson = JsonSerializer.Serialize(command.Answers.Select(a => new
        {
            a.QuestionId,
            a.SelectedOptionId
        }).ToList());
        homeworkStudent.TextAnswer = answersJson;

        var maxScore = homeworkStudent.Assignment.MaxScore ?? totalPoints;
        homeworkStudent.Score = totalPoints > 0
            ? (decimal)earnedPoints / totalPoints * maxScore
            : 0;

        var attempt = new HomeworkSubmissionAttempt
        {
            Id = Guid.NewGuid(),
            HomeworkStudentId = homeworkStudent.Id,
            AttemptNumber = currentAttemptCount + 1,
            Status = homeworkStudent.Status,
            StartedAt = homeworkStudent.StartedAt,
            SubmittedAt = homeworkStudent.SubmittedAt,
            GradedAt = homeworkStudent.GradedAt,
            Score = homeworkStudent.Score,
            TeacherFeedback = homeworkStudent.TeacherFeedback,
            AiFeedback = homeworkStudent.AiFeedback,
            TextAnswer = homeworkStudent.TextAnswer,
            AttachmentUrl = homeworkStudent.AttachmentUrl,
            CreatedAt = now
        };
        context.HomeworkSubmissionAttempts.Add(attempt);

        var isOnTime = !homeworkStudent.Assignment.DueAt.HasValue || now <= homeworkStudent.Assignment.DueAt.Value;
        if (isFirstSubmission && isOnTime)
        {
            var activeHomeworkStreakMissions = await context.MissionProgresses
                .Include(mp => mp.Mission)
                .Where(mp => mp.StudentProfileId == studentId.Value)
                .Where(mp => mp.Mission.MissionType == MissionType.HomeworkStreak)
                .Where(mp => mp.Status == MissionProgressStatus.Assigned ||
                             mp.Status == MissionProgressStatus.InProgress)
                .Where(mp => mp.Mission.StartAt == null || mp.Mission.StartAt <= now)
                .Where(mp => mp.Mission.EndAt == null || mp.Mission.EndAt >= now)
                .ToListAsync(cancellationToken);

            foreach (var missionProgress in activeHomeworkStreakMissions)
            {
                if (missionProgress.Status == MissionProgressStatus.Assigned)
                {
                    missionProgress.Status = MissionProgressStatus.InProgress;
                }

                missionProgress.ProgressValue = (missionProgress.ProgressValue ?? 0) + 1;

                var totalRequired = missionProgress.Mission.TotalRequired;
                if (totalRequired.HasValue && missionProgress.ProgressValue >= totalRequired.Value)
                {
                    missionProgress.Status = MissionProgressStatus.Completed;
                    missionProgress.CompletedAt = now;

                    if (missionProgress.Mission.RewardStars.HasValue &&
                        missionProgress.Mission.RewardStars.Value > 0)
                    {
                        await gamificationService.AddStarsForMissionCompletion(
                            studentId.Value,
                            missionProgress.Mission.RewardStars.Value,
                            missionProgress.MissionId,
                            reason: $"Completed HomeworkStreak Mission: {missionProgress.Mission.Title}",
                            cancellationToken);
                    }

                    if (missionProgress.Mission.RewardExp.HasValue &&
                        missionProgress.Mission.RewardExp.Value > 0)
                    {
                        await gamificationService.AddXpForMissionCompletion(
                            studentId.Value,
                            missionProgress.Mission.RewardExp.Value,
                            missionProgress.MissionId,
                            reason: $"Completed HomeworkStreak Mission: {missionProgress.Mission.Title}",
                            cancellationToken);
                    }
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        var rewardStars = 0;
        if (isFirstSubmission &&
            isOnTime &&
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
            AttemptId = attempt.Id,
            AttemptNumber = attempt.AttemptNumber,
            AttemptCount = attempt.AttemptNumber,
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
