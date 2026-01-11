using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.CreateExamResultsBulk;

public sealed class CreateExamResultsBulkCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateExamResultsBulkCommand, CreateExamResultsBulkResponse>
{
    public async Task<Result<CreateExamResultsBulkResponse>> Handle(CreateExamResultsBulkCommand command, CancellationToken cancellationToken)
    {
        // Check if exam exists
        var exam = await context.Exams
            .FirstOrDefaultAsync(e => e.Id == command.ExamId, cancellationToken);

        if (exam == null)
        {
            return Result.Failure<CreateExamResultsBulkResponse>(
                ExamErrors.NotFound(command.ExamId));
        }

        // Get current user
        var gradedBy = userContext.UserId;

        var createdResults = new List<ExamResultResponse>();
        var errors = new List<string>();
        int skippedCount = 0;

        // Get all student profile IDs to check existence
        var studentProfileIds = command.Results.Select(r => r.StudentProfileId).ToList();
        var existingProfiles = await context.Profiles
            .Where(p => studentProfileIds.Contains(p.Id) &&
                       p.ProfileType == ProfileType.Student &&
                       p.IsActive &&
                       !p.IsDeleted)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // Get existing exam results to avoid duplicates
        var existingExamResults = await context.ExamResults
            .Where(er => er.ExamId == command.ExamId &&
                        studentProfileIds.Contains(er.StudentProfileId))
            .Select(er => er.StudentProfileId)
            .ToListAsync(cancellationToken);

        var examResultsToAdd = new List<ExamResult>();

        foreach (var resultItem in command.Results)
        {
            // Check if student profile exists
            if (!existingProfiles.TryGetValue(resultItem.StudentProfileId, out var studentProfile))
            {
                errors.Add($"Student profile {resultItem.StudentProfileId} not found or inactive");
                skippedCount++;
                continue;
            }

            // Check if exam result already exists
            if (existingExamResults.Contains(resultItem.StudentProfileId))
            {
                errors.Add($"Exam result already exists for student {studentProfile.DisplayName}");
                skippedCount++;
                continue;
            }

            // Serialize attachment URLs to JSON
            string? attachmentUrlsJson = null;
            if (resultItem.AttachmentUrls != null && resultItem.AttachmentUrls.Any())
            {
                attachmentUrlsJson = JsonSerializer.Serialize(resultItem.AttachmentUrls);
            }

            var now = DateTime.UtcNow;
            var examResult = new ExamResult
            {
                Id = Guid.NewGuid(),
                ExamId = command.ExamId,
                StudentProfileId = resultItem.StudentProfileId,
                Score = resultItem.Score,
                Comment = resultItem.Comment,
                AttachmentUrls = attachmentUrlsJson,
                GradedBy = gradedBy,
                GradedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            examResultsToAdd.Add(examResult);

            createdResults.Add(new ExamResultResponse
            {
                Id = examResult.Id,
                StudentProfileId = examResult.StudentProfileId,
                StudentName = studentProfile.DisplayName,
                Score = examResult.Score
            });
        }

        if (examResultsToAdd.Any())
        {
            context.ExamResults.AddRange(examResultsToAdd);
            await context.SaveChangesAsync(cancellationToken);
        }

        return new CreateExamResultsBulkResponse
        {
            CreatedCount = createdResults.Count,
            SkippedCount = skippedCount,
            CreatedResults = createdResults,
            Errors = errors
        };
    }
}

