using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetStudentHomeworks;

public sealed class GetStudentHomeworksQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetStudentHomeworksQuery, GetStudentHomeworksResponse>
{
    public async Task<Result<GetStudentHomeworksResponse>> Handle(
        GetStudentHomeworksQuery query,
        CancellationToken cancellationToken)
    {
        // Get StudentId from context
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetStudentHomeworksResponse>(ProfileErrors.StudentNotFound);
        }

        // Verify student exists
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentId.Value, cancellationToken);

        if (profile == null || profile.ProfileType != Domain.Users.ProfileType.Student || !profile.IsActive || profile.IsDeleted)
        {
            return Result.Failure<GetStudentHomeworksResponse>(ProfileErrors.StudentNotFound);
        }

        // Get homework submissions for this student
        var homeworkQuery = context.HomeworkStudents
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.Class)
            .Where(hs => hs.StudentProfileId == studentId.Value)
            .AsQueryable();

        // Filter by status
        if (query.Status.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(hs => hs.Status == query.Status.Value);
        }

        // Filter by class
        if (query.ClassId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(hs => hs.Assignment.ClassId == query.ClassId.Value);
        }

        // Get total count
        int totalCount = await homeworkQuery.CountAsync(cancellationToken);

        var now = DateTime.UtcNow;

        // Apply pagination and select
        var homeworks = await homeworkQuery
            .OrderByDescending(hs => hs.Assignment.DueAt)
            .ThenByDescending(hs => hs.Assignment.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(hs => new StudentHomeworkDto
            {
                Id = hs.Id,
                AssignmentId = hs.AssignmentId,
                AssignmentTitle = hs.Assignment.Title,
                AssignmentDescription = hs.Assignment.Description,
                ClassId = hs.Assignment.ClassId,
                ClassCode = hs.Assignment.Class.Code,
                ClassTitle = hs.Assignment.Class.Title,
                DueAt = hs.Assignment.DueAt,
                Book = hs.Assignment.Book,
                Pages = hs.Assignment.Pages,
                Skills = hs.Assignment.Skills,
                SubmissionType = SubmissionTypeMapper.ToApiString(hs.Assignment.SubmissionType),
                MaxScore = hs.Assignment.MaxScore,
                Status = hs.Status.ToString(),
                SubmittedAt = hs.SubmittedAt,
                GradedAt = hs.GradedAt,
                Score = hs.Score,
                IsLate = hs.Status == HomeworkStatus.Late,
                IsOverdue = hs.Assignment.DueAt.HasValue && 
                            now > hs.Assignment.DueAt.Value && 
                            (hs.Status == HomeworkStatus.Assigned || hs.Status == HomeworkStatus.Missing)
            })
            .ToListAsync(cancellationToken);

        var page = new Page<StudentHomeworkDto>(
            homeworks,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetStudentHomeworksResponse
        {
            Homeworks = page
        };
    }
}

