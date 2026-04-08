using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.CreateMultipleChoiceHomework;
using Kidzgo.Application.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using QuizOptionUtils = Kidzgo.Application.Homework.Shared.QuizOptionUtils;
namespace Kidzgo.Application.Homework.CreateMultipleChoiceHomeworkFromBank;

public sealed class CreateMultipleChoiceHomeworkFromBankCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateMultipleChoiceHomeworkFromBankCommand, CreateMultipleChoiceHomeworkResponse>
{
    public async Task<Result<CreateMultipleChoiceHomeworkResponse>> Handle(
        CreateMultipleChoiceHomeworkFromBankCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidTitle);
        }

        if (command.Distribution == null || command.Distribution.Count == 0 ||
            command.Distribution.Any(d => d.Count <= 0))
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidQuestionDistribution);
        }

        var programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.ProgramNotFound(command.ProgramId));
        }

        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.ClassNotFound);
        }

        if (command.SessionId.HasValue)
        {
            var session = await context.Sessions
                .FirstOrDefaultAsync(s => s.Id == command.SessionId.Value, cancellationToken);

            if (session is null || session.ClassId != command.ClassId)
            {
                return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.SessionNotFound(command.SessionId));
            }
        }

        if (command.MissionId.HasValue)
        {
            var mission = await context.Missions
                .FirstOrDefaultAsync(m => m.Id == command.MissionId.Value, cancellationToken);

            if (mission is null)
            {
                return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.MissionNotFound(command.MissionId));
            }
        }

        if (command.DueAt.HasValue && VietnamTime.NormalizeToUtc(command.DueAt.Value) <= VietnamTime.UtcNow())
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidDueDate);
        }

        if (command.TimeLimitMinutes.HasValue && command.TimeLimitMinutes.Value <= 0)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidTimeLimitMinutes);
        }

        if (command.MaxAttempts <= 0)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidMaxAttempts);
        }

        var distribution = command.Distribution
            .GroupBy(d => d.Level)
            .Select(g => new { Level = g.Key, Count = g.Sum(x => x.Count) })
            .ToList();

        var levels = distribution
            .Select(d => d.Level)
            .ToList();

        var bankItems = await context.QuestionBankItems
            .Where(q => q.ProgramId == command.ProgramId &&
                        levels.Contains(q.Level) &&
                        q.QuestionType == HomeworkQuestionType.MultipleChoice)
            .ToListAsync(cancellationToken);

        var grouped = bankItems
            .GroupBy(q => q.Level)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var dist in distribution)
        {
            grouped.TryGetValue(dist.Level, out var list);
            var available = list?.Count ?? 0;
            if (available < dist.Count)
            {
                return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.InsufficientQuestionsInBank(dist.Level, dist.Count, available));
            }
        }

        var random = new Random();
        var selected = new List<QuestionBankItem>();

        foreach (var dist in distribution)
        {
            var pool = grouped[dist.Level];
            Shuffle(pool, random);
            selected.AddRange(pool.Take(dist.Count));
        }

        Shuffle(selected, random);

        var dueAtUtc = VietnamTime.NormalizeToUtc(command.DueAt);

        var now = VietnamTime.UtcNow();

        var maxScore = selected.Sum(q => q.Points);

        var homework = new HomeworkAssignment
        {
            Id = Guid.NewGuid(),
            ClassId = command.ClassId,
            SessionId = command.SessionId,
            Title = command.Title,
            Description = command.Description,
            DueAt = dueAtUtc,
            Topic = command.Topic,
            GrammarTags = StringListJson.Serialize(command.GrammarTags),
            VocabularyTags = StringListJson.Serialize(command.VocabularyTags),
            SubmissionType = SubmissionType.Quiz,
            MaxScore = maxScore,
            RewardStars = command.RewardStars,
            TimeLimitMinutes = command.TimeLimitMinutes,
            MaxAttempts = command.MaxAttempts,
            AiHintEnabled = command.AiHintEnabled ?? false,
            AiRecommendEnabled = command.AiRecommendEnabled ?? false,
            MissionId = command.MissionId,
            Instructions = command.Instructions,
            CreatedBy = userContext.UserId,
            CreatedAt = now
        };

        context.HomeworkAssignments.Add(homework);

        var questionDtos = new List<HomeworkQuestionDto>();
        for (int i = 0; i < selected.Count; i++)
        {
            var bank = selected[i];
            var options = bank.Options == null
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(bank.Options) ?? new List<string>();

            var question = new HomeworkQuestion
            {
                Id = Guid.NewGuid(),
                HomeworkAssignmentId = homework.Id,
                OrderIndex = i,
                QuestionText = bank.QuestionText,
                QuestionType = bank.QuestionType,
                Options = bank.Options,
                CorrectAnswer = bank.QuestionType == HomeworkQuestionType.MultipleChoice
                    ? QuizOptionUtils.NormalizeCorrectAnswerForStorage(options, bank.CorrectAnswer) ?? bank.CorrectAnswer
                    : bank.CorrectAnswer?.Trim(),
                Points = bank.Points,
                Explanation = bank.Explanation
            };

            context.HomeworkQuestions.Add(question);

            questionDtos.Add(new Kidzgo.Application.Homework.CreateMultipleChoiceHomework.HomeworkQuestionDto
            {
                Id = question.Id,
                OrderIndex = i,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType.ToString(),
                Options = options,
                Points = question.Points
            });
        }

        var activeEnrollments = await context.ClassEnrollments
            .Where(e => e.ClassId == command.ClassId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.StudentProfileId)
            .ToListAsync(cancellationToken);

        if (activeEnrollments.Count == 0)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.ClassHasNoActiveStudents(command.ClassId));
        }

        var homeworkStudents = activeEnrollments.Select(studentId => new HomeworkStudent
        {
            Id = Guid.NewGuid(),
            AssignmentId = homework.Id,
            StudentProfileId = studentId,
            Status = HomeworkStatus.Assigned
        }).ToList();

        context.HomeworkStudents.AddRange(homeworkStudents);

        await context.SaveChangesAsync(cancellationToken);

        return new CreateMultipleChoiceHomeworkResponse
        {
            Id = homework.Id,
            ClassId = homework.ClassId,
            SessionId = homework.SessionId,
            Title = homework.Title,
            Description = homework.Description,
            DueAt = homework.DueAt,
            Topic = homework.Topic,
            GrammarTags = StringListJson.Deserialize(homework.GrammarTags),
            VocabularyTags = StringListJson.Deserialize(homework.VocabularyTags),
            RewardStars = homework.RewardStars,
            TimeLimitMinutes = homework.TimeLimitMinutes,
            AllowResubmit = homework.MaxAttempts > 1,
            MaxAttempts = homework.MaxAttempts,
            AiHintEnabled = homework.AiHintEnabled,
            AiRecommendEnabled = homework.AiRecommendEnabled,
            Instructions = homework.Instructions,
            CreatedAt = homework.CreatedAt,
            AssignedStudentsCount = homeworkStudents.Count,
            Questions = questionDtos
        };
    }

    private static void Shuffle<T>(IList<T> list, Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
