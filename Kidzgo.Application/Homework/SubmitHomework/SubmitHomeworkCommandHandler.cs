using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Homework.SubmitHomework;

public sealed class SubmitHomeworkCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<SubmitHomeworkCommand, SubmitHomeworkResponse>
{
    public async Task<Result<SubmitHomeworkResponse>> Handle(
        SubmitHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        // Get StudentId from context
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<SubmitHomeworkResponse>(ProfileErrors.StudentNotFound);
        }

        // Get homework submission
        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .FirstOrDefaultAsync(hs => hs.Id == command.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionNotFound(command.HomeworkStudentId));
        }

        // Verify the submission belongs to the current student
        if (homeworkStudent.StudentProfileId != studentId.Value)
        {
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }

        // Check if already submitted or graded
        if (homeworkStudent.Status == HomeworkStatus.Submitted || homeworkStudent.Status == HomeworkStatus.Graded)
        {
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionAlreadySubmitted);
        }

        // Cannot submit if status is MISSING
        if (homeworkStudent.Status == HomeworkStatus.Missing)
        {
            return Result.Failure<SubmitHomeworkResponse>(
                HomeworkErrors.SubmissionCannotSubmitMissing);
        }

        // Validate submission data based on submission type
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
                // Quiz submissions might be handled differently, but for now allow any submission
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

        // Check if due date has passed
        if (homeworkStudent.Assignment.DueAt.HasValue && DateTime.UtcNow > homeworkStudent.Assignment.DueAt.Value)
        {
            homeworkStudent.Status = HomeworkStatus.Late;
        }
        else
        {
            homeworkStudent.Status = HomeworkStatus.Submitted;
        }

        // Set submission data
        homeworkStudent.SubmittedAt = DateTime.UtcNow;

        // Store submission data based on type
        switch (submissionType)
        {
            case SubmissionType.File:
            case SubmissionType.Image:
                if (command.AttachmentUrls != null && command.AttachmentUrls.Count > 0)
                {
                    homeworkStudent.Attachments = JsonSerializer.Serialize(command.AttachmentUrls);
                }
                break;

            case SubmissionType.Text:
                if (!string.IsNullOrWhiteSpace(command.TextAnswer))
                {
                    var textData = new { text = command.TextAnswer };
                    homeworkStudent.Attachments = JsonSerializer.Serialize(textData);
                }
                break;

            case SubmissionType.Link:
                if (!string.IsNullOrWhiteSpace(command.LinkUrl))
                {
                    var linkData = new { url = command.LinkUrl };
                    homeworkStudent.Attachments = JsonSerializer.Serialize(linkData);
                }
                break;

            case SubmissionType.Quiz:
                // For quiz, store all provided data
                var quizData = new
                {
                    text = command.TextAnswer,
                    attachments = command.AttachmentUrls,
                    link = command.LinkUrl
                };
                homeworkStudent.Attachments = JsonSerializer.Serialize(quizData);
                break;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SubmitHomeworkResponse
        {
            Id = homeworkStudent.Id,
            AssignmentId = homeworkStudent.AssignmentId,
            Status = homeworkStudent.Status.ToString(),
            SubmittedAt = homeworkStudent.SubmittedAt!.Value
        };
    }
}

