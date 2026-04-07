using Kidzgo.Application.Abstraction.Homework;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Homework.Shared;

internal static class HomeworkAiContextMapper
{
    public static AiHomeworkContext BuildContext(HomeworkAssignment assignment, Guid studentId)
    {
        return new AiHomeworkContext
        {
            HomeworkId = assignment.Id.ToString(),
            StudentId = studentId.ToString(),
            Subject = "english",
            Skill = InferSkill(assignment),
            Instructions = assignment.Instructions,
            Rubric = assignment.Rubric,
            Topic = assignment.Topic,
            GrammarTags = StringListJson.Deserialize(assignment.GrammarTags),
            VocabularyTags = StringListJson.Deserialize(assignment.VocabularyTags),
            SubmissionType = assignment.SubmissionType.ToString(),
            SpeakingMode = assignment.SpeakingMode,
            TargetWords = StringListJson.Deserialize(assignment.TargetWords)
        };
    }

    public static string? GetExpectedAnswer(HomeworkAssignment assignment)
        => !string.IsNullOrWhiteSpace(assignment.SpeakingExpectedText)
            ? assignment.SpeakingExpectedText
            : assignment.ExpectedAnswer;

    public static HomeworkQuestionType? GetPreferredQuestionType(HomeworkAssignment assignment)
        => assignment.SubmissionType switch
        {
            SubmissionType.Quiz => HomeworkQuestionType.MultipleChoice,
            SubmissionType.Text => HomeworkQuestionType.TextInput,
            SubmissionType.Link => HomeworkQuestionType.TextInput,
            SubmissionType.File => HomeworkQuestionType.TextInput,
            SubmissionType.Image => HomeworkQuestionType.TextInput,
            SubmissionType.Video => HomeworkQuestionType.TextInput,
            _ => null
        };

    private static string InferSkill(HomeworkAssignment assignment)
    {
        if (!string.IsNullOrWhiteSpace(assignment.SpeakingMode))
        {
            return assignment.SpeakingMode!;
        }

        var parsedSkills = StringListJson.ParseTags(assignment.Skills);
        if (parsedSkills.Count > 0)
        {
            return parsedSkills[0];
        }

        return assignment.SubmissionType switch
        {
            SubmissionType.Quiz => "grammar",
            SubmissionType.Text => "writing",
            SubmissionType.Link => "writing",
            SubmissionType.File => "writing",
            SubmissionType.Image => "writing",
            SubmissionType.Video => "writing",
            _ => "mixed"
        };
    }
}
