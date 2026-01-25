using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.GetStudentExamResults;

public sealed class GetStudentExamResultsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetStudentExamResultsQuery, GetStudentExamResultsResponse>
{
    public async Task<Result<GetStudentExamResultsResponse>> Handle(GetStudentExamResultsQuery query, CancellationToken cancellationToken)
    {
        // Get StudentId from context (token)
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetStudentExamResultsResponse>(ProfileErrors.StudentNotFound);
        }

        // Check if student profile exists and belongs to current user
        var studentProfile = await context.Profiles
            .FirstOrDefaultAsync(
                p => p.Id == studentId.Value &&
                     p.UserId == userContext.UserId &&
                     p.ProfileType == ProfileType.Student &&
                     p.IsActive &&
                     !p.IsDeleted,
                cancellationToken);

        if (studentProfile == null)
        {
            return Result.Failure<GetStudentExamResultsResponse>(
                ExamErrors.StudentProfileNotFound);
        }

        var examResultsQuery = context.ExamResults
            .Include(er => er.Exam)
                .ThenInclude(e => e.Class)
            .Where(er => er.StudentProfileId == studentId.Value)
            .AsQueryable();

        // Filter by exam type
        if (query.ExamType.HasValue)
        {
            examResultsQuery = examResultsQuery.Where(er => er.Exam.ExamType == query.ExamType.Value);
        }

        // Get total count
        int totalCount = await examResultsQuery.CountAsync(cancellationToken);

        // Apply pagination and load data
        var examResultsData = await examResultsQuery
            .OrderByDescending(er => er.Exam.Date)
            .ThenByDescending(er => er.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs and deserialize JSON in memory
        var examResults = examResultsData.Select(er => new StudentExamResultDto
        {
            Id = er.Id,
            ExamId = er.ExamId,
            ExamType = er.Exam.ExamType.ToString(),
            ExamDate = er.Exam.Date,
            ClassCode = er.Exam.Class.Code,
            ClassTitle = er.Exam.Class.Title,
            Score = er.Score,
            MaxScore = er.Exam.MaxScore,
            Comment = er.Comment,
            AttachmentUrls = !string.IsNullOrEmpty(er.AttachmentUrls)
                ? JsonSerializer.Deserialize<List<string>>(er.AttachmentUrls)
                : null,
            GradedAt = er.GradedAt,
            CreatedAt = er.CreatedAt
        }).ToList();

        var page = new Page<StudentExamResultDto>(
            examResults,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetStudentExamResultsResponse
        {
            ExamResults = page
        };
    }
}

