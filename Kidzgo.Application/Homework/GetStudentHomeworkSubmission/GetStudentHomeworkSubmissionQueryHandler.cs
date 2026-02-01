using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Homework.GetStudentHomeworkSubmission;

public sealed class GetStudentHomeworkSubmissionQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetStudentHomeworkSubmissionQuery, GetStudentHomeworkSubmissionResponse>
{
    public async Task<Result<GetStudentHomeworkSubmissionResponse>> Handle(
        GetStudentHomeworkSubmissionQuery query,
        CancellationToken cancellationToken)
    {
        // Get StudentId from context
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetStudentHomeworkSubmissionResponse>(ProfileErrors.StudentNotFound);
        }

        // Get homework submission
        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.Class)
            .FirstOrDefaultAsync(hs => hs.Id == query.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<GetStudentHomeworkSubmissionResponse>(
                HomeworkErrors.SubmissionNotFound(query.HomeworkStudentId));
        }

        // Verify the submission belongs to the current student
        if (homeworkStudent.StudentProfileId != studentId.Value)
        {
            return Result.Failure<GetStudentHomeworkSubmissionResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }

        // Parse attachments
        List<string>? attachmentUrls = null;
        string? textAnswer = null;
        string? linkUrl = null;

        if (!string.IsNullOrWhiteSpace(homeworkStudent.Attachments))
        {
            try
            {
                if (homeworkStudent.Assignment.SubmissionType == SubmissionType.Text)
                {
                    var textData = JsonSerializer.Deserialize<JsonElement>(homeworkStudent.Attachments);
                    if (textData.TryGetProperty("text", out var textProp))
                    {
                        textAnswer = textProp.GetString();
                    }
                }
                else if (homeworkStudent.Assignment.SubmissionType == SubmissionType.Link)
                {
                    var linkData = JsonSerializer.Deserialize<JsonElement>(homeworkStudent.Attachments);
                    if (linkData.TryGetProperty("url", out var urlProp))
                    {
                        linkUrl = urlProp.GetString();
                    }
                }
                else
                {
                    attachmentUrls = JsonSerializer.Deserialize<List<string>>(homeworkStudent.Attachments);
                }
            }
            catch
            {
                // If parsing fails, leave as null
            }
        }

        var now = DateTime.UtcNow;
        var isOverdue = homeworkStudent.Assignment.DueAt.HasValue && 
                       now > homeworkStudent.Assignment.DueAt.Value && 
                       (homeworkStudent.Status == HomeworkStatus.Assigned || homeworkStudent.Status == HomeworkStatus.Missing);

        return new GetStudentHomeworkSubmissionResponse
        {
            Id = homeworkStudent.Id,
            AssignmentId = homeworkStudent.AssignmentId,
            AssignmentTitle = homeworkStudent.Assignment.Title,
            AssignmentDescription = homeworkStudent.Assignment.Description,
            Instructions = homeworkStudent.Assignment.Instructions,
            ClassId = homeworkStudent.Assignment.ClassId,
            ClassCode = homeworkStudent.Assignment.Class.Code,
            ClassTitle = homeworkStudent.Assignment.Class.Title,
            DueAt = homeworkStudent.Assignment.DueAt,
            Book = homeworkStudent.Assignment.Book,
            Pages = homeworkStudent.Assignment.Pages,
            Skills = homeworkStudent.Assignment.Skills,
            SubmissionType = homeworkStudent.Assignment.SubmissionType.ToString(),
            MaxScore = homeworkStudent.Assignment.MaxScore,
            Status = homeworkStudent.Status.ToString(),
            SubmittedAt = homeworkStudent.SubmittedAt,
            GradedAt = homeworkStudent.GradedAt,
            Score = homeworkStudent.Score,
            TeacherFeedback = homeworkStudent.TeacherFeedback,
            AiFeedback = homeworkStudent.AiFeedback,
            AttachmentUrls = attachmentUrls,
            TextAnswer = textAnswer,
            LinkUrl = linkUrl,
            IsLate = homeworkStudent.Status == HomeworkStatus.Late,
            IsOverdue = isOverdue
        };
    }
}

