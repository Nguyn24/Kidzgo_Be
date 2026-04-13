using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Exams.Errors;
using Kidzgo.Domain.Homework;
using Kidzgo.Application.QuestionBank.Shared;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Exams.CreateExamQuestionsFromBankMatrix;

using QuizOptionUtils = Kidzgo.Application.Homework.Shared.QuizOptionUtils;

public sealed class CreateExamQuestionsFromBankMatrixCommandHandler(
    IDbContext context
) : ICommandHandler<CreateExamQuestionsFromBankMatrixCommand, CreateExamQuestionsFromBankMatrixResponse>
{
    public async Task<Result<CreateExamQuestionsFromBankMatrixResponse>> Handle(
        CreateExamQuestionsFromBankMatrixCommand command,
        CancellationToken cancellationToken)
    {
        if (command.TotalQuestions <= 0)
        {
            return Result.Failure<CreateExamQuestionsFromBankMatrixResponse>(
                Error.Validation("ExamQuestion.InvalidTotalQuestions", "Total questions must be greater than 0"));
        }

        if (command.Distribution is null || command.Distribution.Count == 0 || command.Distribution.All(x => x.Count <= 0))
        {
            return Result.Failure<CreateExamQuestionsFromBankMatrixResponse>(ExamQuestionErrors.InvalidMatrixDistribution);
        }

        var normalizedDistribution = command.Distribution
            .Where(x => x.Count > 0)
            .GroupBy(x => x.Level)
            .Select(g => new ExamQuestionMatrixLevelCountDto
            {
                Level = g.Key,
                Count = g.Sum(x => x.Count)
            })
            .ToList();

        var distributedTotal = normalizedDistribution.Sum(x => x.Count);
        if (distributedTotal != command.TotalQuestions)
        {
            return Result.Failure<CreateExamQuestionsFromBankMatrixResponse>(
                ExamQuestionErrors.MatrixTotalMismatch(command.TotalQuestions, distributedTotal));
        }

        var bankQuestionType = MapQuestionType(command.QuestionType);
        if (!bankQuestionType.HasValue)
        {
            return Result.Failure<CreateExamQuestionsFromBankMatrixResponse>(ExamQuestionErrors.UnsupportedQuestionBankType);
        }

        var exam = await context.Exams
            .Include(e => e.Class)
            .FirstOrDefaultAsync(e => e.Id == command.ExamId, cancellationToken);

        if (exam is null)
        {
            return Result.Failure<CreateExamQuestionsFromBankMatrixResponse>(ExamQuestionErrors.ExamNotFound(command.ExamId));
        }

        var hasSubmissions = await context.ExamSubmissions
            .AsNoTracking()
            .AnyAsync(s => s.ExamId == command.ExamId, cancellationToken);

        if (hasSubmissions)
        {
            return Result.Failure<CreateExamQuestionsFromBankMatrixResponse>(
                ExamQuestionErrors.CannotRegenerateWhenSubmissionsExist);
        }

        var existingQuestions = await context.ExamQuestions
            .Where(q => q.ExamId == command.ExamId)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(cancellationToken);

        var previousQuestionCount = existingQuestions.Count;
        var selection = await QuestionBankMatrixSelector.SelectAsync(
            context,
            new QuestionBankMatrixSelectionRequest
            {
                ProgramId = exam.Class.ProgramId,
                QuestionType = bankQuestionType.Value,
                Skill = command.Skill,
                Topic = command.Topic,
                ShuffleQuestions = command.ShuffleQuestions,
                Distribution = normalizedDistribution
                    .Select(x => new QuestionBankMatrixLevelCount
                    {
                        Level = x.Level,
                        Count = x.Count
                    })
                    .ToList()
            },
            cancellationToken);

        if (selection.InsufficientLevel.HasValue)
        {
            return Result.Failure<CreateExamQuestionsFromBankMatrixResponse>(
                ExamQuestionErrors.InsufficientQuestionsInBank(
                    selection.InsufficientLevel.Value,
                    selection.RequiredCount,
                    selection.AvailableCount));
        }

        var selectedItems = selection.SelectedItems;

        if (command.ReplaceExistingQuestions && existingQuestions.Count > 0)
        {
            context.ExamQuestions.RemoveRange(existingQuestions);
        }

        var now = VietnamTime.UtcNow();
        var nextOrderIndex = command.ReplaceExistingQuestions
            ? 1
            : (existingQuestions.LastOrDefault()?.OrderIndex ?? 0) + 1;

        var createdQuestions = new List<(ExamQuestion Question, QuestionBankItem Source)>();

        foreach (var bankItem in selectedItems)
        {
            var options = QuizOptionUtils.ParseOptions(bankItem.Options);
            var normalizedCorrectAnswer = bankQuestionType == HomeworkQuestionType.MultipleChoice
                ? QuizOptionUtils.NormalizeCorrectAnswerForStorage(options, bankItem.CorrectAnswer) ?? bankItem.CorrectAnswer
                : bankItem.CorrectAnswer?.Trim();

            var examQuestion = new ExamQuestion
            {
                Id = Guid.NewGuid(),
                ExamId = exam.Id,
                OrderIndex = nextOrderIndex++,
                QuestionText = bankItem.QuestionText,
                QuestionType = command.QuestionType,
                Options = bankItem.Options,
                CorrectAnswer = normalizedCorrectAnswer,
                Points = bankItem.Points,
                Explanation = bankItem.Explanation,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.ExamQuestions.Add(examQuestion);
            createdQuestions.Add((examQuestion, bankItem));
        }

        var existingPoints = command.ReplaceExistingQuestions
            ? 0
            : existingQuestions.Sum(q => q.Points);

        exam.MaxScore = existingPoints + createdQuestions.Sum(x => x.Question.Points);
        exam.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new CreateExamQuestionsFromBankMatrixResponse
        {
            ExamId = exam.Id,
            ClassId = exam.ClassId,
            ProgramId = exam.Class.ProgramId,
            ExamType = exam.ExamType.ToString(),
            QuestionType = command.QuestionType.ToString(),
            Skill = command.Skill,
            Topic = command.Topic,
            RequestedQuestionCount = command.TotalQuestions,
            CreatedQuestionCount = createdQuestions.Count,
            TotalPoints = createdQuestions.Sum(x => x.Question.Points),
            ReplacedExistingQuestions = command.ReplaceExistingQuestions,
            PreviousQuestionCount = previousQuestionCount,
            Distribution = normalizedDistribution
                .Select(level => new ExamQuestionMatrixDistributionDto
                {
                    Level = level.Level.ToString(),
                    RequestedCount = level.Count,
                    CreatedCount = createdQuestions.Count(x => x.Source.Level == level.Level)
                })
                .ToList(),
            Questions = createdQuestions
                .Select(item => new GeneratedExamQuestionDto
                {
                    Id = item.Question.Id,
                    SourceQuestionBankItemId = item.Source.Id,
                    OrderIndex = item.Question.OrderIndex,
                    QuestionText = item.Question.QuestionText,
                    QuestionType = item.Question.QuestionType.ToString(),
                    Level = item.Source.Level.ToString(),
                    Points = item.Question.Points,
                    Options = QuizOptionUtils.ParseOptions(item.Question.Options),
                    CorrectAnswer = item.Question.CorrectAnswer,
                    Explanation = item.Question.Explanation
                })
                .OrderBy(q => q.OrderIndex)
                .ToList()
        };
    }

    private static HomeworkQuestionType? MapQuestionType(QuestionType questionType)
    {
        return questionType switch
        {
            QuestionType.MultipleChoice => HomeworkQuestionType.MultipleChoice,
            QuestionType.Text => HomeworkQuestionType.TextInput,
            _ => null
        };
    }

}
