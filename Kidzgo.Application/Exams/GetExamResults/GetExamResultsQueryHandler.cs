using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.GetExamResults;

public sealed class GetExamResultsQueryHandler(
    IDbContext context
) : IQueryHandler<GetExamResultsQuery, GetExamResultsResponse>
{
    public async Task<Result<GetExamResultsResponse>> Handle(GetExamResultsQuery query, CancellationToken cancellationToken)
    {
        var examResultsQuery = context.ExamResults
            .Include(er => er.Exam)
            .Include(er => er.StudentProfile)
            .Include(er => er.GradedByUser)
            .AsQueryable();

        // Filter by examId
        if (query.ExamId.HasValue)
        {
            examResultsQuery = examResultsQuery.Where(er => er.ExamId == query.ExamId.Value);
        }

        // Filter by studentProfileId
        if (query.StudentProfileId.HasValue)
        {
            examResultsQuery = examResultsQuery.Where(er => er.StudentProfileId == query.StudentProfileId.Value);
        }

        // Get total count
        int totalCount = await examResultsQuery.CountAsync(cancellationToken);

        // Apply pagination and load data
        var examResultsData = await examResultsQuery
            .OrderByDescending(er => er.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs and deserialize JSON in memory
        var examResults = examResultsData.Select(er => new ExamResultDto
        {
            Id = er.Id,
            ExamId = er.ExamId,
            ExamType = er.Exam.ExamType.ToString(),
            ExamDate = er.Exam.Date,
            StudentProfileId = er.StudentProfileId,
            StudentName = er.StudentProfile.DisplayName,
            Score = er.Score,
            Comment = er.Comment,
            AttachmentUrls = !string.IsNullOrEmpty(er.AttachmentUrls)
                ? JsonSerializer.Deserialize<List<string>>(er.AttachmentUrls)
                : null,
            GradedBy = er.GradedBy,
            GradedByName = er.GradedByUser != null ? er.GradedByUser.Name : null,
            GradedAt = er.GradedAt,
            CreatedAt = er.CreatedAt
        }).ToList();

        var page = new Page<ExamResultDto>(
            examResults,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetExamResultsResponse
        {
            ExamResults = page
        };
    }
}

