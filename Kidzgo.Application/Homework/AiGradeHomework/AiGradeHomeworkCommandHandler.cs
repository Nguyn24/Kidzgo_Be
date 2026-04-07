using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Homework;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.AiGradeHomework;

public sealed class AiGradeHomeworkCommandHandler(
    IDbContext context,
    IAiHomeworkAssistant aiHomeworkAssistant
) : ICommandHandler<AiGradeHomeworkCommand, AiGradeHomeworkResponse>
{
    public async Task<Result<AiGradeHomeworkResponse>> Handle(
        AiGradeHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .FirstOrDefaultAsync(hs => hs.Id == command.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<AiGradeHomeworkResponse>(
                HomeworkErrors.SubmissionNotFound(command.HomeworkStudentId));
        }

        if (homeworkStudent.Status != HomeworkStatus.Submitted &&
            homeworkStudent.Status != HomeworkStatus.Graded)
        {
            return Result.Failure<AiGradeHomeworkResponse>(
                HomeworkErrors.SubmissionNotSubmitted);
        }

        if (string.IsNullOrWhiteSpace(homeworkStudent.TextAnswer) &&
            string.IsNullOrWhiteSpace(homeworkStudent.AttachmentUrl))
        {
            return Result.Failure<AiGradeHomeworkResponse>(
                HomeworkErrors.SubmissionInvalidData(homeworkStudent.Assignment.SubmissionType.ToString()));
        }

        var aiContext = HomeworkAiContextMapper.BuildContext(
            homeworkStudent.Assignment,
            homeworkStudent.StudentProfileId);

        if (!string.IsNullOrWhiteSpace(command.Instructions))
        {
            aiContext.Instructions = command.Instructions;
        }

        if (!string.IsNullOrWhiteSpace(command.Rubric))
        {
            aiContext.Rubric = command.Rubric;
        }

        if (!string.IsNullOrWhiteSpace(homeworkStudent.Assignment.SpeakingMode))
        {
            var speakingResult = await aiHomeworkAssistant.AnalyzeSpeakingSubmissionAsync(
                new AiHomeworkSpeakingSubmissionRequest
                {
                    Context = aiContext,
                    Transcript = homeworkStudent.TextAnswer,
                    AttachmentUrl = homeworkStudent.AttachmentUrl,
                    ExpectedText = !string.IsNullOrWhiteSpace(command.ExpectedAnswerText)
                        ? command.ExpectedAnswerText
                        : HomeworkAiContextMapper.GetExpectedAnswer(homeworkStudent.Assignment),
                    Language = string.IsNullOrWhiteSpace(command.Language) ? "vi" : command.Language
                },
                cancellationToken);

            var persistedSpeaking = speakingResult.AiUsed;
            if (persistedSpeaking)
            {
                homeworkStudent.Status = HomeworkStatus.Graded;
                homeworkStudent.Score = NormalizeScore(speakingResult, homeworkStudent.Assignment);
                homeworkStudent.AiFeedback = JsonSerializer.Serialize(
                    speakingResult,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                homeworkStudent.GradedAt = DateTime.UtcNow;

                var latestAttempt = await context.HomeworkSubmissionAttempts
                    .Where(a => a.HomeworkStudentId == homeworkStudent.Id)
                    .OrderByDescending(a => a.AttemptNumber)
                    .FirstOrDefaultAsync(cancellationToken);

                if (latestAttempt != null)
                {
                    latestAttempt.Status = homeworkStudent.Status;
                    latestAttempt.Score = homeworkStudent.Score;
                    latestAttempt.AiFeedback = homeworkStudent.AiFeedback;
                    latestAttempt.GradedAt = homeworkStudent.GradedAt;
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            return new AiGradeHomeworkResponse
            {
                Id = homeworkStudent.Id,
                AssignmentId = homeworkStudent.AssignmentId,
                IsSpeakingAnalysis = true,
                AiUsed = speakingResult.AiUsed,
                Persisted = persistedSpeaking,
                Status = (persistedSpeaking ? HomeworkStatus.Graded : homeworkStudent.Status).ToString(),
                Score = homeworkStudent.Score,
                RawAiScore = (decimal)speakingResult.Result.OverallScore,
                RawAiMaxScore = 10m,
                Summary = speakingResult.Result.Summary,
                Strengths = speakingResult.Result.Strengths,
                Issues = speakingResult.Result.PhonicsIssues
                    .Concat(speakingResult.Result.SpeakingIssues)
                    .ToList(),
                Suggestions = speakingResult.Result.Suggestions,
                ExtractedStudentAnswer = speakingResult.Result.Transcript,
                Stars = speakingResult.Result.Stars,
                PronunciationScore = (decimal)speakingResult.Result.PronunciationScore,
                FluencyScore = (decimal)speakingResult.Result.FluencyScore,
                AccuracyScore = (decimal)speakingResult.Result.AccuracyScore,
                MispronouncedWords = speakingResult.Result.MispronouncedWords,
                WordFeedback = speakingResult.Result.WordFeedback
                    .Select(x => new SpeakingWordFeedbackDto
                    {
                        Word = x.Word,
                        HeardAs = x.HeardAs,
                        Issue = x.Issue,
                        Tip = x.Tip
                    })
                    .ToList(),
                PracticePlan = speakingResult.Result.PracticePlan,
                Confidence = speakingResult.Result.Confidence,
                Warnings = speakingResult.Result.Warnings,
                GradedAt = homeworkStudent.GradedAt
            };
        }

        var aiResult = await aiHomeworkAssistant.GradeSubmissionAsync(
            new AiHomeworkGradeSubmissionRequest
            {
                Context = aiContext,
                StudentAnswerText = homeworkStudent.TextAnswer,
                AttachmentUrl = homeworkStudent.AttachmentUrl,
                ExpectedAnswerText = !string.IsNullOrWhiteSpace(command.ExpectedAnswerText)
                    ? command.ExpectedAnswerText
                    : HomeworkAiContextMapper.GetExpectedAnswer(homeworkStudent.Assignment),
                Language = string.IsNullOrWhiteSpace(command.Language) ? "vi" : command.Language
            },
            cancellationToken);

        var persisted = aiResult.AiUsed;
        if (persisted)
        {
            homeworkStudent.Status = HomeworkStatus.Graded;
            homeworkStudent.Score = NormalizeScore(aiResult, homeworkStudent.Assignment);
            homeworkStudent.AiFeedback = JsonSerializer.Serialize(
                aiResult,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            homeworkStudent.GradedAt = DateTime.UtcNow;

            var latestAttempt = await context.HomeworkSubmissionAttempts
                .Where(a => a.HomeworkStudentId == homeworkStudent.Id)
                .OrderByDescending(a => a.AttemptNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestAttempt != null)
            {
                latestAttempt.Status = homeworkStudent.Status;
                latestAttempt.Score = homeworkStudent.Score;
                latestAttempt.AiFeedback = homeworkStudent.AiFeedback;
                latestAttempt.GradedAt = homeworkStudent.GradedAt;
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        return new AiGradeHomeworkResponse
        {
            Id = homeworkStudent.Id,
            AssignmentId = homeworkStudent.AssignmentId,
            IsSpeakingAnalysis = false,
            AiUsed = aiResult.AiUsed,
            Persisted = persisted,
            Status = (persisted ? HomeworkStatus.Graded : homeworkStudent.Status).ToString(),
            Score = homeworkStudent.Score,
            RawAiScore = (decimal)aiResult.Result.Score,
            RawAiMaxScore = (decimal)aiResult.Result.MaxScore,
            Summary = aiResult.Result.Summary,
            Strengths = aiResult.Result.Strengths,
            Issues = aiResult.Result.Issues,
            Suggestions = aiResult.Result.Suggestions,
            ExtractedStudentAnswer = aiResult.Result.ExtractedStudentAnswer,
            Confidence = aiResult.Result.Confidence,
            Warnings = aiResult.Result.Warnings,
            GradedAt = homeworkStudent.GradedAt
        };
    }

    private static decimal NormalizeScore(
        AiHomeworkGradeResult aiResult,
        HomeworkAssignment assignment)
    {
        var rawMaxScore = aiResult.Result.MaxScore > 0
            ? (decimal)aiResult.Result.MaxScore
            : 10m;
        var rawScore = Math.Clamp((decimal)aiResult.Result.Score, 0m, rawMaxScore);
        var targetMaxScore = assignment.MaxScore ?? rawMaxScore;

        if (targetMaxScore <= 0)
        {
            return rawScore;
        }

        var scaledScore = rawMaxScore <= 0
            ? rawScore
            : rawScore / rawMaxScore * targetMaxScore;

        return Math.Round(scaledScore, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal NormalizeScore(
        AiHomeworkSpeakingResult aiResult,
        HomeworkAssignment assignment)
    {
        var rawMaxScore = 10m;
        var rawScore = Math.Clamp((decimal)aiResult.Result.OverallScore, 0m, rawMaxScore);
        var targetMaxScore = assignment.MaxScore ?? rawMaxScore;

        if (targetMaxScore <= 0)
        {
            return rawScore;
        }

        var scaledScore = rawScore / rawMaxScore * targetMaxScore;
        return Math.Round(scaledScore, 2, MidpointRounding.AwayFromZero);
    }
}
