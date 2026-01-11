using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.UpdateExamResult;

public sealed class UpdateExamResultCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateExamResultCommand, UpdateExamResultResponse>
{
    public async Task<Result<UpdateExamResultResponse>> Handle(UpdateExamResultCommand command, CancellationToken cancellationToken)
    {
        var examResult = await context.ExamResults
            .Include(er => er.StudentProfile)
            .FirstOrDefaultAsync(er => er.Id == command.Id, cancellationToken);

        if (examResult == null)
        {
            return Result.Failure<UpdateExamResultResponse>(
                ExamErrors.ExamResultNotFound);
        }

        // Update fields if provided
        if (command.Score.HasValue)
        {
            examResult.Score = command.Score;
        }

        if (command.Comment != null)
        {
            examResult.Comment = command.Comment;
        }

        if (command.AttachmentUrls != null)
        {
            examResult.AttachmentUrls = command.AttachmentUrls.Any()
                ? JsonSerializer.Serialize(command.AttachmentUrls)
                : null;
        }

        examResult.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        // Deserialize attachment URLs
        List<string>? attachmentUrls = null;
        if (!string.IsNullOrEmpty(examResult.AttachmentUrls))
        {
            attachmentUrls = JsonSerializer.Deserialize<List<string>>(examResult.AttachmentUrls);
        }

        return new UpdateExamResultResponse
        {
            Id = examResult.Id,
            ExamId = examResult.ExamId,
            StudentProfileId = examResult.StudentProfileId,
            StudentName = examResult.StudentProfile.DisplayName,
            Score = examResult.Score,
            Comment = examResult.Comment,
            AttachmentUrls = attachmentUrls,
            UpdatedAt = examResult.UpdatedAt
        };
    }
}

