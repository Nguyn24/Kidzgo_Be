using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
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

        if (homeworkStudent.Status == HomeworkStatus.Submitted || homeworkStudent.Status == HomeworkStatus.Graded)
        {
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionAlreadySubmitted);
        }

        var submissionType = homeworkStudent.Assignment.SubmissionType;
        bool hasValidSubmission = false;

        switch (submissionType)
        {
            case SubmissionType.File:
            case SubmissionType.Image:
                hasValidSubmission = command.AttachmentUrls != null && command.AttachmentUrls.Count > 0;
                break;
            case SubmissionType.Text:
                hasValidSubmission = !string.IsNullOrWhiteSpace(command.TextAnswer);
                break;
            case SubmissionType.Link:
                hasValidSubmission = !string.IsNullOrWhiteSpace(command.LinkUrl);
                break;
            case SubmissionType.Quiz:
                hasValidSubmission = command.AttachmentUrls != null && command.AttachmentUrls.Count > 0 ||
                                    !string.IsNullOrWhiteSpace(command.TextAnswer) ||
                                    !string.IsNullOrWhiteSpace(command.LinkUrl);
                break;
        }

        if (!hasValidSubmission)
        {
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionInvalidData(submissionType.ToString()));
        }

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

        // Store submission data based on type
        switch (submissionType)
        {
            case SubmissionType.File:
            case SubmissionType.Image:
                if (command.AttachmentUrls != null && command.AttachmentUrls.Count > 0)
                {
                    homeworkStudent.AttachmentUrl = command.AttachmentUrls[0];
                }
                break;

            case SubmissionType.Text:
                homeworkStudent.TextAnswer = command.TextAnswer;
                break;

            case SubmissionType.Link:
                homeworkStudent.AttachmentUrl = command.LinkUrl;
                break;

            case SubmissionType.Quiz:
                if (!string.IsNullOrWhiteSpace(command.TextAnswer))
                {
                    homeworkStudent.TextAnswer = command.TextAnswer;
                }
                if (!string.IsNullOrWhiteSpace(command.LinkUrl))
                {
                    homeworkStudent.AttachmentUrl = command.LinkUrl;
                }
                break;
        }

        // ============================================================
        // GIAI DOAN 2: Track HomeworkStreak Mission progress
        // Chỉ increment progress khi nộp đúng hạn (Submitted)
        // ============================================================
        if (homeworkStudent.Status == HomeworkStatus.Submitted)
        {
            var now = DateTime.UtcNow;

            // Tim tat ca HomeworkStreak missions đang active của student
            // (đã có MissionProgress record, trong khoảng thời gian, chưa hoàn thành)
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
                // Update status to InProgress if currently Assigned
                if (missionProgress.Status == MissionProgressStatus.Assigned)
                {
                    missionProgress.Status = MissionProgressStatus.InProgress;
                }

                // Increment progress value
                missionProgress.ProgressValue = (missionProgress.ProgressValue ?? 0) + 1;

                // Check if mission is completed (reached TotalRequired)
                var totalRequired = missionProgress.Mission.TotalRequired;
                if (totalRequired.HasValue && missionProgress.ProgressValue >= totalRequired.Value)
                {
                    missionProgress.Status = MissionProgressStatus.Completed;
                    missionProgress.CompletedAt = now;

                    // Cộng Mission Reward Stars
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

                    // Cộng Mission Reward XP
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

        // UC-208: Cộng Homework Reward Stars (đúng hạn)
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

        return new SubmitHomeworkResponse
        {
            Id = homeworkStudent.Id,
            AssignmentId = homeworkStudent.AssignmentId,
            Status = homeworkStudent.Status.ToString(),
            SubmittedAt = homeworkStudent.SubmittedAt!.Value
        };
    }
}
