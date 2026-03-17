using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

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
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetStudentHomeworkSubmissionResponse>(ProfileErrors.StudentNotFound);
        }

        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.Class)
            .FirstOrDefaultAsync(hs => hs.Id == query.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<GetStudentHomeworkSubmissionResponse>(
                HomeworkErrors.SubmissionNotFound(query.HomeworkStudentId));
        }

        if (homeworkStudent.StudentProfileId != studentId.Value)
        {
            return Result.Failure<GetStudentHomeworkSubmissionResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }
        
        var now = DateTime.UtcNow;
        var isOverdue = homeworkStudent.Assignment.DueAt.HasValue && 
                       now > homeworkStudent.Assignment.DueAt.Value && 
                       (homeworkStudent.Status == HomeworkStatus.Assigned || homeworkStudent.Status == HomeworkStatus.Missing);

        List<StudentHomeworkQuestionDto> questions = new();
        if (homeworkStudent.Assignment.SubmissionType == SubmissionType.Quiz)
        {
            var homeworkQuestions = await context.HomeworkQuestions
                .Where(q => q.HomeworkAssignmentId == homeworkStudent.AssignmentId)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync(cancellationToken);

            foreach (var question in homeworkQuestions)
            {
                List<string> options = new();
                if (!string.IsNullOrWhiteSpace(question.Options))
                {
                    try
                    {
                        options = JsonSerializer.Deserialize<List<string>>(question.Options) ?? new List<string>();
                    }
                    catch
                    {
                        options = new List<string>();
                    }
                }

                questions.Add(new StudentHomeworkQuestionDto
                {
                    Id = question.Id,
                    OrderIndex = question.OrderIndex,
                    QuestionText = question.QuestionText,
                    QuestionType = question.QuestionType.ToString(),
                    Options = options,
                    Points = question.Points
                });
            }
        }

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
            RewardStars = homeworkStudent.Assignment.RewardStars,
            Status = homeworkStudent.Status.ToString(),
            SubmittedAt = homeworkStudent.SubmittedAt,
            GradedAt = homeworkStudent.GradedAt,
            Score = homeworkStudent.Score,
            TeacherFeedback = homeworkStudent.TeacherFeedback,
            AiFeedback = homeworkStudent.AiFeedback,
            AttachmentUrls = homeworkStudent.AttachmentUrl,
            TextAnswer = homeworkStudent.TextAnswer,
            IsLate = homeworkStudent.Status == HomeworkStatus.Late,
            IsOverdue = isOverdue,
            Questions = questions
        };
    }
}
