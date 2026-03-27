using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
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

        await context.SaveChangesAsync(cancellationToken);

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
