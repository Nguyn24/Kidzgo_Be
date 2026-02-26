using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetHomeworkSubmissions;

public sealed class GetHomeworkSubmissionsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetHomeworkSubmissionsQuery, GetHomeworkSubmissionsResponse>
{
    public async Task<Result<GetHomeworkSubmissionsResponse>> Handle(
        GetHomeworkSubmissionsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        // Get class IDs where this user is MainTeacher or AssistantTeacher
        var teacherClassIds = await context.Classes
            .Where(c => c.MainTeacherId == userId || c.AssistantTeacherId == userId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (!teacherClassIds.Any())
        {
            return Result.Success(new GetHomeworkSubmissionsResponse
            {
                Submissions = new Page<HomeworkSubmissionDto>(
                    new List<HomeworkSubmissionDto>(),
                    0,
                    query.PageNumber,
                    query.PageSize)
            });
        }

        // Base query: get homework submissions from teacher's classes
        var submissionsQuery = context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .Include(hs => hs.StudentProfile)
                .ThenInclude(p => p.User!)
            .Where(hs => teacherClassIds.Contains(hs.Assignment.ClassId))
            .AsQueryable();

        // Filter by class if provided
        if (query.ClassId.HasValue)
        {
            // Ensure the class belongs to this teacher
            if (!teacherClassIds.Contains(query.ClassId.Value))
            {
                return Result.Success(new GetHomeworkSubmissionsResponse
                {
                    Submissions = new Page<HomeworkSubmissionDto>(
                        new List<HomeworkSubmissionDto>(),
                        0,
                        query.PageNumber,
                        query.PageSize)
                });
            }

            submissionsQuery = submissionsQuery.Where(hs => hs.Assignment.ClassId == query.ClassId.Value);
        }

        // Filter by status
        if (query.Status.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(hs => hs.Status == query.Status.Value);
        }

        // Get total count
        int totalCount = await submissionsQuery.CountAsync(cancellationToken);

        // Apply pagination and select
        var submissions = await submissionsQuery
            .OrderByDescending(hs => hs.SubmittedAt ?? hs.Assignment.DueAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(hs => new HomeworkSubmissionDto
            {
                Id = hs.Id,
                HomeworkAssignmentId = hs.AssignmentId,
                HomeworkTitle = hs.Assignment.Title,
                StudentProfileId = hs.StudentProfileId,
                StudentName = hs.StudentProfile.DisplayName,
                Status = hs.Status.ToString(),
                SubmittedAt = hs.SubmittedAt,
                GradedAt = hs.GradedAt,
                Score = hs.Score,
                TeacherFeedback = hs.TeacherFeedback,
                DueAt = hs.Assignment.DueAt ?? DateTime.MinValue,
                CreatedAt = hs.Assignment.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<HomeworkSubmissionDto>(
            submissions,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetHomeworkSubmissionsResponse
        {
            Submissions = page
        };
    }
}

