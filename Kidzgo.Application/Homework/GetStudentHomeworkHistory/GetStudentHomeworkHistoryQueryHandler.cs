using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetStudentHomeworkHistory;

public sealed class GetStudentHomeworkHistoryQueryHandler(
    IDbContext context
) : IQueryHandler<GetStudentHomeworkHistoryQuery, GetStudentHomeworkHistoryResponse>
{
    public async Task<Result<GetStudentHomeworkHistoryResponse>> Handle(
        GetStudentHomeworkHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var homeworkQuery = context.HomeworkStudents
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.Class)
            .Where(hs => hs.StudentProfileId == query.StudentProfileId)
            .AsQueryable();

        // Filter by class
        if (query.ClassId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(hs => hs.Assignment.ClassId == query.ClassId.Value);
        }

        // Get total count
        int totalCount = await homeworkQuery.CountAsync(cancellationToken);

        // Apply pagination and select
        var homeworks = await homeworkQuery
            .OrderByDescending(hs => hs.Assignment.CreatedAt)
            .ThenByDescending(hs => hs.SubmittedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(hs => new StudentHomeworkHistoryDto
            {
                Id = hs.Id,
                AssignmentId = hs.AssignmentId,
                AssignmentTitle = hs.Assignment.Title,
                ClassId = hs.Assignment.ClassId,
                ClassCode = hs.Assignment.Class.Code,
                ClassTitle = hs.Assignment.Class.Title,
                DueAt = hs.Assignment.DueAt,
                Status = hs.Status.ToString(),
                SubmittedAt = hs.SubmittedAt,
                GradedAt = hs.GradedAt,
                Score = hs.Score,
                MaxScore = hs.Assignment.MaxScore,
                TeacherFeedback = hs.TeacherFeedback,
                IsLate = hs.Status == HomeworkStatus.Late
            })
            .ToListAsync(cancellationToken);

        var page = new Page<StudentHomeworkHistoryDto>(
            homeworks,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetStudentHomeworkHistoryResponse
        {
            Homeworks = page
        };
    }
}

