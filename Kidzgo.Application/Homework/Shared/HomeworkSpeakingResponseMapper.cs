using Kidzgo.Application.Abstraction.Homework;
using Kidzgo.Application.Homework.GetHomeworkSpeakingAnalysis;

namespace Kidzgo.Application.Homework.Shared;

internal static class HomeworkSpeakingResponseMapper
{
    public static GetHomeworkSpeakingAnalysisResponse ToResponse(AiHomeworkSpeakingResult aiResult)
    {
        return new GetHomeworkSpeakingAnalysisResponse
        {
            AiUsed = aiResult.AiUsed,
            Summary = aiResult.Result.Summary,
            Transcript = aiResult.Result.Transcript,
            OverallScore = (decimal)aiResult.Result.OverallScore,
            PronunciationScore = (decimal)aiResult.Result.PronunciationScore,
            FluencyScore = (decimal)aiResult.Result.FluencyScore,
            AccuracyScore = (decimal)aiResult.Result.AccuracyScore,
            Stars = aiResult.Result.Stars,
            Strengths = aiResult.Result.Strengths,
            Issues = aiResult.Result.PhonicsIssues
                .Concat(aiResult.Result.SpeakingIssues)
                .ToList(),
            MispronouncedWords = aiResult.Result.MispronouncedWords,
            WordFeedback = aiResult.Result.WordFeedback
                .Select(x => new HomeworkSpeakingWordFeedbackResponse
                {
                    Word = x.Word,
                    HeardAs = x.HeardAs,
                    Issue = x.Issue,
                    Tip = x.Tip
                })
                .ToList(),
            Suggestions = aiResult.Result.Suggestions,
            PracticePlan = aiResult.Result.PracticePlan,
            Confidence = aiResult.Result.Confidence,
            Warnings = aiResult.Result.Warnings
        };
    }
}
