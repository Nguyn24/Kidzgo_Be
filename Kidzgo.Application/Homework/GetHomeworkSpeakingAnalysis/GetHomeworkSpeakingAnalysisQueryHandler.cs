using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Homework;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetHomeworkSpeakingAnalysis;

public sealed class GetHomeworkSpeakingAnalysisQueryHandler(
    IDbContext context,
    IUserContext userContext,
    IAiHomeworkAssistant aiHomeworkAssistant
) : IQueryHandler<GetHomeworkSpeakingAnalysisQuery, GetHomeworkSpeakingAnalysisResponse>
{
    public async Task<Result<GetHomeworkSpeakingAnalysisResponse>> Handle(
        GetHomeworkSpeakingAnalysisQuery query,
        CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetHomeworkSpeakingAnalysisResponse>(ProfileErrors.StudentNotFound);
        }

        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .FirstOrDefaultAsync(hs => hs.Id == query.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<GetHomeworkSpeakingAnalysisResponse>(
                HomeworkErrors.SubmissionNotFound(query.HomeworkStudentId));
        }

        if (homeworkStudent.StudentProfileId != studentId.Value)
        {
            return Result.Failure<GetHomeworkSpeakingAnalysisResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }

        if (string.IsNullOrWhiteSpace(homeworkStudent.Assignment.SpeakingMode))
        {
            return Result.Failure<GetHomeworkSpeakingAnalysisResponse>(
                HomeworkErrors.AiSpeakingNotAvailable);
        }

        var transcript = !string.IsNullOrWhiteSpace(query.CurrentTranscript)
            ? query.CurrentTranscript
            : homeworkStudent.TextAnswer;

        if (string.IsNullOrWhiteSpace(transcript) &&
            string.IsNullOrWhiteSpace(homeworkStudent.AttachmentUrl))
        {
            return Result.Failure<GetHomeworkSpeakingAnalysisResponse>(
                HomeworkErrors.SubmissionInvalidData(homeworkStudent.Assignment.SubmissionType.ToString()));
        }

        var aiResult = await aiHomeworkAssistant.AnalyzeSpeakingSubmissionAsync(
            new AiHomeworkSpeakingSubmissionRequest
            {
                Context = HomeworkAiContextMapper.BuildContext(homeworkStudent.Assignment, studentId.Value),
                Transcript = transcript,
                AttachmentUrl = homeworkStudent.AttachmentUrl,
                ExpectedText = HomeworkAiContextMapper.GetExpectedAnswer(homeworkStudent.Assignment),
                Language = string.IsNullOrWhiteSpace(query.Language) ? "vi" : query.Language
            },
            cancellationToken);

        return HomeworkSpeakingResponseMapper.ToResponse(aiResult);
    }
}
