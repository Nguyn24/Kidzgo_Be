using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.GetExamResultById;

public sealed class GetExamResultByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetExamResultByIdQuery, GetExamResultByIdResponse>
{
    public async Task<Result<GetExamResultByIdResponse>> Handle(GetExamResultByIdQuery query, CancellationToken cancellationToken)
    {
        var examResult = await context.ExamResults
            .Include(er => er.Exam)
            .Include(er => er.StudentProfile)
            .Include(er => er.GradedByUser)
            .FirstOrDefaultAsync(er => er.Id == query.Id, cancellationToken);

        if (examResult == null)
        {
            return Result.Failure<GetExamResultByIdResponse>(
                ExamErrors.ExamResultNotFound);
        }

        // Deserialize attachment URLs
        List<string>? attachmentUrls = null;
        if (!string.IsNullOrEmpty(examResult.AttachmentUrls))
        {
            attachmentUrls = JsonSerializer.Deserialize<List<string>>(examResult.AttachmentUrls);
        }

        return new GetExamResultByIdResponse
        {
            Id = examResult.Id,
            ExamId = examResult.ExamId,
            ExamType = examResult.Exam.ExamType.ToString(),
            ExamDate = examResult.Exam.Date,
            StudentProfileId = examResult.StudentProfileId,
            StudentName = examResult.StudentProfile.DisplayName,
            Score = examResult.Score,
            Comment = examResult.Comment,
            AttachmentUrls = attachmentUrls,
            GradedBy = examResult.GradedBy,
            GradedByName = examResult.GradedByUser != null ? examResult.GradedByUser.Name : null,
            GradedAt = examResult.GradedAt,
            CreatedAt = examResult.CreatedAt
        };
    }
}

