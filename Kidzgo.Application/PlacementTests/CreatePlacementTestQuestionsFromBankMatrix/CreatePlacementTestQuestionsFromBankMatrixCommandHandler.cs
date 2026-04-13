using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.QuestionBank.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Homework;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.CreatePlacementTestQuestionsFromBankMatrix;

using QuizOptionUtils = Kidzgo.Application.Homework.Shared.QuizOptionUtils;

public sealed class CreatePlacementTestQuestionsFromBankMatrixCommandHandler(
    IDbContext context
) : ICommandHandler<CreatePlacementTestQuestionsFromBankMatrixCommand, CreatePlacementTestQuestionsFromBankMatrixResponse>
{
    public async Task<Result<CreatePlacementTestQuestionsFromBankMatrixResponse>> Handle(
        CreatePlacementTestQuestionsFromBankMatrixCommand command,
        CancellationToken cancellationToken)
    {
        if (command.TotalQuestions <= 0)
        {
            return Result.Failure<CreatePlacementTestQuestionsFromBankMatrixResponse>(
                Error.Validation("PlacementTest.InvalidTotalQuestions", "Total questions must be greater than 0"));
        }

        if (command.Distribution is null || command.Distribution.Count == 0 || command.Distribution.All(x => x.Count <= 0))
        {
            return Result.Failure<CreatePlacementTestQuestionsFromBankMatrixResponse>(
                PlacementTestErrors.InvalidQuestionMatrixDistribution);
        }

        var normalizedDistribution = command.Distribution
            .Where(x => x.Count > 0)
            .GroupBy(x => x.Level)
            .Select(g => new PlacementTestQuestionMatrixLevelCountDto
            {
                Level = g.Key,
                Count = g.Sum(x => x.Count)
            })
            .ToList();

        var distributedTotal = normalizedDistribution.Sum(x => x.Count);
        if (distributedTotal != command.TotalQuestions)
        {
            return Result.Failure<CreatePlacementTestQuestionsFromBankMatrixResponse>(
                PlacementTestErrors.MatrixTotalMismatch(command.TotalQuestions, distributedTotal));
        }

        var placementTest = await context.PlacementTests
            .Include(pt => pt.Class)
            .FirstOrDefaultAsync(pt => pt.Id == command.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<CreatePlacementTestQuestionsFromBankMatrixResponse>(
                PlacementTestErrors.NotFound(command.PlacementTestId));
        }

        var resolvedProgramId = command.ProgramId
            ?? placementTest.ProgramRecommendationId
            ?? placementTest.Class?.ProgramId;

        if (!resolvedProgramId.HasValue)
        {
            return Result.Failure<CreatePlacementTestQuestionsFromBankMatrixResponse>(
                PlacementTestErrors.ProgramNotResolved(command.PlacementTestId));
        }

        var programExists = await context.Programs
            .AsNoTracking()
            .AnyAsync(p => p.Id == resolvedProgramId.Value && p.IsActive && !p.IsDeleted, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<CreatePlacementTestQuestionsFromBankMatrixResponse>(
                PlacementTestErrors.ProgramNotResolved(command.PlacementTestId));
        }

        var selection = await QuestionBankMatrixSelector.SelectAsync(
            context,
            new QuestionBankMatrixSelectionRequest
            {
                ProgramId = resolvedProgramId.Value,
                QuestionType = command.QuestionType,
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
            return Result.Failure<CreatePlacementTestQuestionsFromBankMatrixResponse>(
                PlacementTestErrors.InsufficientQuestionsInBank(
                    selection.InsufficientLevel.Value,
                    selection.RequiredCount,
                    selection.AvailableCount));
        }

        var programSource = command.ProgramId.HasValue
            ? "Request"
            : placementTest.ProgramRecommendationId.HasValue
                ? "PlacementTest.ProgramRecommendation"
                : "PlacementTest.Class.Program";

        var questions = selection.SelectedItems
            .Select((item, index) => new GeneratedPlacementTestQuestionDto
            {
                SourceQuestionBankItemId = item.Id,
                OrderIndex = index + 1,
                QuestionText = item.QuestionText,
                QuestionType = item.QuestionType.ToString(),
                Level = item.Level.ToString(),
                Points = item.Points,
                Options = QuizOptionUtils.ParseOptions(item.Options),
                CorrectAnswer = item.CorrectAnswer,
                Explanation = item.Explanation
            })
            .ToList();

        return new CreatePlacementTestQuestionsFromBankMatrixResponse
        {
            PlacementTestId = placementTest.Id,
            ProgramId = resolvedProgramId.Value,
            ProgramSource = programSource,
            QuestionType = command.QuestionType.ToString(),
            Skill = command.Skill,
            Topic = command.Topic,
            RequestedQuestionCount = selection.RequestedQuestionCount,
            CreatedQuestionCount = questions.Count,
            TotalPoints = questions.Sum(x => x.Points),
            Distribution = normalizedDistribution
                .Select(level => new PlacementTestQuestionMatrixDistributionDto
                {
                    Level = level.Level.ToString(),
                    RequestedCount = level.Count,
                    CreatedCount = questions.Count(x => string.Equals(x.Level, level.Level.ToString(), StringComparison.Ordinal))
                })
                .ToList(),
            Questions = questions
        };
    }
}
