using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.CreateExamResult;

public sealed class CreateExamResultCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateExamResultCommand, CreateExamResultResponse>
{
    public async Task<Result<CreateExamResultResponse>> Handle(CreateExamResultCommand command, CancellationToken cancellationToken)
    {
        // Check if exam exists
        var exam = await context.Exams
            .FirstOrDefaultAsync(e => e.Id == command.ExamId, cancellationToken);

        if (exam == null)
        {
            return Result.Failure<CreateExamResultResponse>(
                ExamErrors.NotFound(command.ExamId));
        }

        // Check if student profile exists
        var studentProfile = await context.Profiles
            .FirstOrDefaultAsync(
                p => p.Id == command.StudentProfileId &&
                     p.ProfileType == ProfileType.Student &&
                     p.IsActive &&
                     !p.IsDeleted,
                cancellationToken);

        if (studentProfile == null)
        {
            return Result.Failure<CreateExamResultResponse>(
                ExamErrors.StudentProfileNotFound);
        }

        // Check if exam result already exists
        bool exists = await context.ExamResults
            .AnyAsync(er => er.ExamId == command.ExamId && er.StudentProfileId == command.StudentProfileId, cancellationToken);

        if (exists)
        {
            return Result.Failure<CreateExamResultResponse>(
                ExamErrors.ExamResultAlreadyExists);
        }

        // Get current user
        var gradedBy = userContext.UserId;

        // Serialize attachment URLs to JSON
        string? attachmentUrlsJson = null;
        if (command.AttachmentUrls != null && command.AttachmentUrls.Any())
        {
            attachmentUrlsJson = JsonSerializer.Serialize(command.AttachmentUrls);
        }

        // Create exam result
        var now = DateTime.UtcNow;
        var examResult = new ExamResult
        {
            Id = Guid.NewGuid(),
            ExamId = command.ExamId,
            StudentProfileId = command.StudentProfileId,
            Score = command.Score,
            Comment = command.Comment,
            AttachmentUrls = attachmentUrlsJson,
            GradedBy = gradedBy,
            GradedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ExamResults.Add(examResult);
        await context.SaveChangesAsync(cancellationToken);

        // Get graded by user name
        var gradedByUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == gradedBy, cancellationToken);

        // Deserialize attachment URLs
        List<string>? attachmentUrls = null;
        if (!string.IsNullOrEmpty(attachmentUrlsJson))
        {
            attachmentUrls = JsonSerializer.Deserialize<List<string>>(attachmentUrlsJson);
        }

        return new CreateExamResultResponse
        {
            Id = examResult.Id,
            ExamId = examResult.ExamId,
            StudentProfileId = examResult.StudentProfileId,
            StudentName = studentProfile.DisplayName,
            Score = examResult.Score,
            Comment = examResult.Comment,
            AttachmentUrls = attachmentUrls,
            GradedBy = examResult.GradedBy,
            GradedByName = gradedByUser?.Name,
            GradedAt = examResult.GradedAt,
            CreatedAt = examResult.CreatedAt
        };
    }
}

