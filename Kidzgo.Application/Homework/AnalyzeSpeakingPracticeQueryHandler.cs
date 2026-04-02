using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Homework;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.GetHomeworkSpeakingAnalysis;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.AnalyzeSpeakingPractice;

public sealed class AnalyzeSpeakingPracticeQueryHandler(
    IDbContext context,
    IUserContext userContext,
    IAiHomeworkAssistant aiHomeworkAssistant
) : IQueryHandler<AnalyzeSpeakingPracticeQuery, GetHomeworkSpeakingAnalysisResponse>
{
    public async Task<Result<GetHomeworkSpeakingAnalysisResponse>> Handle(
        AnalyzeSpeakingPracticeQuery query,
        CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetHomeworkSpeakingAnalysisResponse>(ProfileErrors.StudentNotFound);
        }

        if (query.FileBytes.Length == 0)
        {
            return Result.Failure<GetHomeworkSpeakingAnalysisResponse>(
                HomeworkErrors.AiSpeakingPracticeFileRequired);
        }

        var aiContext = new AiHomeworkContext
        {
            HomeworkId = $"practice-{Guid.NewGuid():N}",
            StudentId = studentId.Value.ToString(),
            Subject = "english",
            Skill = NormalizeMode(query.Mode) ?? "speaking",
            SubmissionType = "file",
            SpeakingMode = NormalizeMode(query.Mode) ?? "speaking",
            TargetWords = StringListJson.ParseTags(query.TargetWords),
            Instructions = NormalizeText(query.Instructions)
        };

        var expectedText = NormalizeText(query.ExpectedText);

        if (query.HomeworkStudentId.HasValue)
        {
            var homeworkStudent = await context.HomeworkStudents
                .Include(hs => hs.Assignment)
                .FirstOrDefaultAsync(hs => hs.Id == query.HomeworkStudentId.Value, cancellationToken);

            if (homeworkStudent is null)
            {
                return Result.Failure<GetHomeworkSpeakingAnalysisResponse>(
                    HomeworkErrors.SubmissionNotFound(query.HomeworkStudentId.Value));
            }

            if (homeworkStudent.StudentProfileId != studentId.Value)
            {
                return Result.Failure<GetHomeworkSpeakingAnalysisResponse>(
                    HomeworkErrors.SubmissionUnauthorized);
            }

            aiContext = HomeworkAiContextMapper.BuildContext(homeworkStudent.Assignment, studentId.Value);
            aiContext.SubmissionType = "file";

            var requestedMode = NormalizeMode(query.Mode);
            if (!string.IsNullOrWhiteSpace(requestedMode))
            {
                aiContext.SpeakingMode = requestedMode;
                aiContext.Skill = requestedMode;
            }
            else if (string.IsNullOrWhiteSpace(aiContext.SpeakingMode))
            {
                aiContext.SpeakingMode = "speaking";
                aiContext.Skill = "speaking";
            }

            var requestedTargetWords = StringListJson.ParseTags(query.TargetWords);
            if (requestedTargetWords.Count > 0)
            {
                aiContext.TargetWords = requestedTargetWords;
            }

            var requestedInstructions = NormalizeText(query.Instructions);
            if (!string.IsNullOrWhiteSpace(requestedInstructions))
            {
                aiContext.Instructions = requestedInstructions;
            }

            if (string.IsNullOrWhiteSpace(expectedText))
            {
                expectedText = HomeworkAiContextMapper.GetExpectedAnswer(homeworkStudent.Assignment);
            }
        }

        var aiResult = await aiHomeworkAssistant.AnalyzeSpeakingMediaAsync(
            new AiHomeworkSpeakingMediaRequest
            {
                Context = aiContext,
                FileBytes = query.FileBytes,
                FileName = string.IsNullOrWhiteSpace(query.FileName) ? "speaking-practice" : query.FileName,
                ContentType = string.IsNullOrWhiteSpace(query.ContentType)
                    ? "application/octet-stream"
                    : query.ContentType,
                ExpectedText = expectedText,
                Language = string.IsNullOrWhiteSpace(query.Language) ? "vi" : query.Language
            },
            cancellationToken);

        return HomeworkSpeakingResponseMapper.ToResponse(aiResult);
    }

    private static string? NormalizeText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeMode(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
