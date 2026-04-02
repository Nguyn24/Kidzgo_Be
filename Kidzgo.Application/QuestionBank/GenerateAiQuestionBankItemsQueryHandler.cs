using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Homework;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.QuestionBank;

public sealed class GenerateAiQuestionBankItemsQueryHandler(
    IDbContext context,
    IAiHomeworkAssistant aiHomeworkAssistant
) : IQueryHandler<GenerateAiQuestionBankItemsQuery, GenerateAiQuestionBankItemsResponse>
{
    public async Task<Result<GenerateAiQuestionBankItemsResponse>> Handle(
        GenerateAiQuestionBankItemsQuery query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Topic))
        {
            return Result.Failure<GenerateAiQuestionBankItemsResponse>(HomeworkErrors.AiCreatorTopicRequired);
        }

        if (query.QuestionCount is < 1 or > 10)
        {
            return Result.Failure<GenerateAiQuestionBankItemsResponse>(
                HomeworkErrors.AiCreatorQuestionCountInvalid(1, 10));
        }

        if (query.PointsPerQuestion <= 0)
        {
            return Result.Failure<GenerateAiQuestionBankItemsResponse>(HomeworkErrors.AiCreatorInvalidPoints);
        }

        var programExists = await context.Programs
            .AnyAsync(p => p.Id == query.ProgramId, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<GenerateAiQuestionBankItemsResponse>(
                HomeworkErrors.ProgramNotFound(query.ProgramId));
        }

        var aiResult = await aiHomeworkAssistant.GenerateQuestionBankItemsAsync(
            new AiQuestionBankGenerationRequest
            {
                ProgramId = query.ProgramId.ToString(),
                Topic = query.Topic,
                QuestionType = query.QuestionType.ToString(),
                QuestionCount = query.QuestionCount,
                Level = query.Level.ToString(),
                Skill = query.Skill,
                TaskStyle = query.TaskStyle,
                GrammarTags = query.GrammarTags,
                VocabularyTags = query.VocabularyTags,
                Instructions = query.Instructions,
                Language = string.IsNullOrWhiteSpace(query.Language) ? "vi" : query.Language,
                PointsPerQuestion = query.PointsPerQuestion
            },
            cancellationToken);

        return new GenerateAiQuestionBankItemsResponse
        {
            AiUsed = aiResult.AiUsed,
            Summary = aiResult.Result.Summary,
            Items = aiResult.Result.Items
                .Select(x => new GeneratedQuestionBankItemDto
                {
                    QuestionText = x.QuestionText,
                    QuestionType = x.QuestionType,
                    Options = x.Options,
                    CorrectAnswer = x.CorrectAnswer,
                    Points = x.Points,
                    Explanation = x.Explanation,
                    Topic = x.Topic,
                    Skill = x.Skill,
                    GrammarTags = x.GrammarTags,
                    VocabularyTags = x.VocabularyTags,
                    Level = x.Level
                })
                .ToList(),
            Warnings = aiResult.Result.Warnings
        };
    }
}
