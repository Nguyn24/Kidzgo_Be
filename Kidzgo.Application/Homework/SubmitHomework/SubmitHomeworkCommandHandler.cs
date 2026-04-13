using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Missions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.SubmitHomework;

public sealed class SubmitHomeworkCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IGamificationService gamificationService
) : ICommandHandler<SubmitHomeworkCommand, SubmitHomeworkResponse>
{
    public async Task<Result<SubmitHomeworkResponse>> Handle(
        SubmitHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<SubmitHomeworkResponse>(ProfileErrors.StudentNotFound);
        }

        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .FirstOrDefaultAsync(hs => hs.Id == command.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionNotFound(command.HomeworkStudentId));
        }

        if (homeworkStudent.StudentProfileId != studentId.Value)
        {
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }

        if (homeworkStudent.Status == HomeworkStatus.Missing &&
            homeworkStudent.SubmittedAt == null &&
            homeworkStudent.GradedAt.HasValue)
        {
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionAlreadyAutoGraded);
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
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionAttemptLimitReached(maxAttempts));
        }

        var submissionType = homeworkStudent.Assignment.SubmissionType;
        var effectiveLink = !string.IsNullOrWhiteSpace(command.LinkUrl)
            ? command.LinkUrl
            : command.Links?.FirstOrDefault();
        var hasValidSubmission = submissionType switch
        {
            SubmissionType.File or SubmissionType.Image or SubmissionType.Video =>
                command.AttachmentUrls != null && command.AttachmentUrls.Count > 0,
            SubmissionType.Text => !string.IsNullOrWhiteSpace(command.TextAnswer),
            SubmissionType.Link => !string.IsNullOrWhiteSpace(effectiveLink),
            SubmissionType.Quiz =>
                command.AttachmentUrls != null && command.AttachmentUrls.Count > 0 ||
                !string.IsNullOrWhiteSpace(command.TextAnswer) ||
                !string.IsNullOrWhiteSpace(effectiveLink),
            _ => false
        };

        if (!hasValidSubmission)
        {
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionInvalidData(submissionType.ToString()));
        }

        var now = VietnamTime.UtcNow();
        var isFirstSubmission = currentAttemptCount == 0;

        homeworkStudent.Status = homeworkStudent.Status == HomeworkStatus.Missing ||
                                 (homeworkStudent.Assignment.DueAt.HasValue &&
                                  now > homeworkStudent.Assignment.DueAt.Value)
            ? HomeworkStatus.Late
            : HomeworkStatus.Submitted;

        homeworkStudent.SubmittedAt = now;
        homeworkStudent.GradedAt = null;
        homeworkStudent.Score = null;
        homeworkStudent.TeacherFeedback = null;
        homeworkStudent.AiFeedback = null;
        homeworkStudent.TextAnswer = null;
        homeworkStudent.AttachmentUrl = null;

        switch (submissionType)
        {
            case SubmissionType.File:
            case SubmissionType.Image:
            case SubmissionType.Video:
                if (command.AttachmentUrls != null && command.AttachmentUrls.Count > 0)
                {
                    homeworkStudent.AttachmentUrl = command.AttachmentUrls[0];
                }
                break;

            case SubmissionType.Text:
                homeworkStudent.TextAnswer = command.TextAnswer;
                break;

            case SubmissionType.Link:
                homeworkStudent.AttachmentUrl = effectiveLink;
                break;

            case SubmissionType.Quiz:
                if (!string.IsNullOrWhiteSpace(command.TextAnswer))
                {
                    homeworkStudent.TextAnswer = command.TextAnswer;
                }

                if (!string.IsNullOrWhiteSpace(effectiveLink))
                {
                    homeworkStudent.AttachmentUrl = effectiveLink;
                }
                break;
        }

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

        await context.SaveChangesAsync(cancellationToken);

        await HomeworkMissionProgressTracker.TrackAsync(
            context,
            gamificationService,
            studentId.Value,
            now,
            cancellationToken);

        if (isFirstSubmission &&
            homeworkStudent.Status == HomeworkStatus.Submitted &&
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

        return new SubmitHomeworkResponse
        {
            Id = homeworkStudent.Id,
            AssignmentId = homeworkStudent.AssignmentId,
            Status = homeworkStudent.Status.ToString(),
            SubmittedAt = homeworkStudent.SubmittedAt!.Value,
            AttemptId = attempt.Id,
            AttemptNumber = attempt.AttemptNumber,
            AttemptCount = attempt.AttemptNumber,
            AttachmentUrls = !string.IsNullOrWhiteSpace(homeworkStudent.AttachmentUrl) &&
                             submissionType is SubmissionType.File or SubmissionType.Image or SubmissionType.Video or SubmissionType.Quiz
                ? new List<string> { homeworkStudent.AttachmentUrl }
                : new List<string>(),
            Links = !string.IsNullOrWhiteSpace(homeworkStudent.AttachmentUrl) &&
                    submissionType == SubmissionType.Link
                ? new List<string> { homeworkStudent.AttachmentUrl }
                : new List<string>()
        };
    }
}
