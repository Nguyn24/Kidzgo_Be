using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.GetExamSubmissions;

public sealed class GetExamSubmissionsQueryHandler(
    IDbContext context
) : IQueryHandler<GetExamSubmissionsQuery, GetExamSubmissionsResponse>
{
    public async Task<Result<GetExamSubmissionsResponse>> Handle(
        GetExamSubmissionsQuery query,
        CancellationToken cancellationToken)
    {
        // Check if exam exists
        var examExists = await context.Exams
            .AnyAsync(e => e.Id == query.ExamId, cancellationToken);

        if (!examExists)
        {
            return Result.Failure<GetExamSubmissionsResponse>(
                ExamSubmissionErrors.ExamNotFound(query.ExamId));
        }

        var submissionsQuery = context.ExamSubmissions
            .Include(s => s.StudentProfile)
            .Include(s => s.GradedByUser)
            .Where(s => s.ExamId == query.ExamId)
            .AsQueryable();

        if (query.StudentProfileId.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.Status.HasValue)
        {
            submissionsQuery = submissionsQuery.Where(s => s.Status == query.Status.Value);
        }

        int totalCount = await submissionsQuery.CountAsync(cancellationToken);

        var submissions = await submissionsQuery
            .OrderByDescending(s => s.SubmittedAt ?? s.AutoSubmittedAt ?? s.ActualStartTime)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(s => new ExamSubmissionDto
            {
                Id = s.Id,
                ExamId = s.ExamId,
                StudentProfileId = s.StudentProfileId,
                StudentName = s.StudentProfile != null ? s.StudentProfile.DisplayName : null,
                ActualStartTime = s.ActualStartTime,
                SubmittedAt = s.SubmittedAt,
                AutoSubmittedAt = s.AutoSubmittedAt,
                TimeSpentMinutes = s.TimeSpentMinutes,
                AutoScore = s.AutoScore,
                FinalScore = s.FinalScore,
                GradedBy = s.GradedBy,
                GradedByName = s.GradedByUser != null ? s.GradedByUser.Name : null,
                GradedAt = s.GradedAt,
                Status = s.Status
            })
            .ToListAsync(cancellationToken);

        var page = new Page<ExamSubmissionDto>(
            submissions,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetExamSubmissionsResponse
        {
            Submissions = page
        };
    }
}


