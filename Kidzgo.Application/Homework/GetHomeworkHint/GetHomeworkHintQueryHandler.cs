using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Homework;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetHomeworkHint;

public sealed class GetHomeworkHintQueryHandler(
    IDbContext context,
    IUserContext userContext,
    IAiHomeworkAssistant aiHomeworkAssistant
) : IQueryHandler<GetHomeworkHintQuery, GetHomeworkHintResponse>
{
    public async Task<Result<GetHomeworkHintResponse>> Handle(
        GetHomeworkHintQuery query,
        CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetHomeworkHintResponse>(ProfileErrors.StudentNotFound);
        }

        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .FirstOrDefaultAsync(hs => hs.Id == query.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<GetHomeworkHintResponse>(
                HomeworkErrors.SubmissionNotFound(query.HomeworkStudentId));
        }

        if (homeworkStudent.StudentProfileId != studentId.Value)
        {
            return Result.Failure<GetHomeworkHintResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }

        if (!homeworkStudent.Assignment.AiHintEnabled)
        {
            return Result.Failure<GetHomeworkHintResponse>(
                HomeworkErrors.AiHintNotEnabled);
        }

        var aiResult = await aiHomeworkAssistant.GetHintAsync(
            new AiHomeworkHintRequest
            {
                Context = HomeworkAiContextMapper.BuildContext(homeworkStudent.Assignment, studentId.Value),
                StudentAnswerText = !string.IsNullOrWhiteSpace(query.CurrentAnswerText)
                    ? query.CurrentAnswerText
                    : homeworkStudent.TextAnswer,
                ExpectedAnswerText = HomeworkAiContextMapper.GetExpectedAnswer(homeworkStudent.Assignment),
                Language = string.IsNullOrWhiteSpace(query.Language) ? "vi" : query.Language
            },
            cancellationToken);

        return new GetHomeworkHintResponse
        {
            AiUsed = aiResult.AiUsed,
            Summary = aiResult.Result.Summary,
            Hints = aiResult.Result.Hints,
            GrammarFocus = aiResult.Result.GrammarFocus,
            VocabularyFocus = aiResult.Result.VocabularyFocus,
            Encouragement = aiResult.Result.Encouragement,
            Warnings = aiResult.Result.Warnings
        };
    }
}
