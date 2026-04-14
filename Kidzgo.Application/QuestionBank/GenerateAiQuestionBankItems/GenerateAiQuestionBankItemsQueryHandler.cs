using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Homework;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.QuestionBank.Shared;
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
        string? sourceText = string.IsNullOrWhiteSpace(query.SourceText)
            ? null
            : query.SourceText.Trim();
        string? sourceFileName = string.IsNullOrWhiteSpace(query.SourceFileName)
            ? null
            : query.SourceFileName.Trim();

        if (query.SourceFileStream is not null)
        {
            var extractionResult = QuestionBankAiSourceExtractor.Extract(
                query.SourceFileStream,
                sourceFileName ?? "question-bank-source");

            if (extractionResult.IsFailure)
            {
                return Result.Failure<GenerateAiQuestionBankItemsResponse>(extractionResult.Error);
            }

            sourceText = extractionResult.Value.Text;
            sourceFileName = extractionResult.Value.FileName;
        }

        if (string.IsNullOrWhiteSpace(query.Topic) && string.IsNullOrWhiteSpace(sourceText))
        {
            return Result.Failure<GenerateAiQuestionBankItemsResponse>(HomeworkErrors.AiCreatorTopicRequired);
        }

        if (query.QuestionCount is < 1 or > 50)
        {
            return Result.Failure<GenerateAiQuestionBankItemsResponse>(
                HomeworkErrors.AiCreatorQuestionCountInvalid(1, 50));
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
                Topic = ResolveTopic(query.Topic, sourceFileName),
                QuestionType = query.QuestionType.ToString(),
                QuestionCount = query.QuestionCount,
                Level = query.Level.ToString(),
                Skill = query.Skill,
                TaskStyle = query.TaskStyle,
                GrammarTags = query.GrammarTags,
                VocabularyTags = query.VocabularyTags,
                Instructions = query.Instructions,
                Language = string.IsNullOrWhiteSpace(query.Language) ? "vi" : query.Language,
                PointsPerQuestion = query.PointsPerQuestion,
                SourceText = sourceText,
                SourceFileName = sourceFileName
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

    private static string ResolveTopic(string topic, string? sourceFileName)
    {
        if (!string.IsNullOrWhiteSpace(topic))
        {
            return topic.Trim();
        }

        if (!string.IsNullOrWhiteSpace(sourceFileName))
        {
            var fileTopic = Path.GetFileNameWithoutExtension(sourceFileName);
            if (!string.IsNullOrWhiteSpace(fileTopic))
            {
                return fileTopic;
            }
        }

        return "source file";
    }
}
