using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetStudentHomeworkFeedback;

public sealed class GetStudentHomeworkFeedbackQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetStudentHomeworkFeedbackQuery, GetStudentHomeworkFeedbackResponse>
{
    public async Task<Result<GetStudentHomeworkFeedbackResponse>> Handle(
        GetStudentHomeworkFeedbackQuery query,
        CancellationToken cancellationToken)
    {
        // Get StudentId from context
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetStudentHomeworkFeedbackResponse>(ProfileErrors.StudentNotFound);
        }

        // Verify student exists
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentId.Value, cancellationToken);

        if (profile == null || profile.ProfileType != Domain.Users.ProfileType.Student || !profile.IsActive || profile.IsDeleted)
        {
            return Result.Failure<GetStudentHomeworkFeedbackResponse>(ProfileErrors.StudentNotFound);
        }

        // Get homework submissions that have been graded (have feedback)
        var homeworkQuery = context.HomeworkStudents
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.Class)
            .Where(hs => hs.StudentProfileId == studentId.Value)
            .Where(hs => hs.Status == HomeworkStatus.Graded || 
                        (!string.IsNullOrWhiteSpace(hs.TeacherFeedback) || 
                         !string.IsNullOrWhiteSpace(hs.AiFeedback)))
            .AsQueryable();

        // Filter by class
        if (query.ClassId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(hs => hs.Assignment.ClassId == query.ClassId.Value);
        }

        // Get total count
        int totalCount = await homeworkQuery.CountAsync(cancellationToken);

        // Apply pagination and select
        var feedbacks = await homeworkQuery
            .OrderByDescending(hs => hs.GradedAt)
            .ThenByDescending(hs => hs.SubmittedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(hs => new StudentHomeworkFeedbackDto
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
                AiFeedback = hs.AiFeedback,
                IsLate = hs.Status == HomeworkStatus.Late
            })
            .ToListAsync(cancellationToken);

        var page = new Page<StudentHomeworkFeedbackDto>(
            feedbacks,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetStudentHomeworkFeedbackResponse
        {
            Feedbacks = page
        };
    }
}

