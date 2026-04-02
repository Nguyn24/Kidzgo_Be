using System.Text.Json.Serialization;

namespace Kidzgo.Application.Abstraction.Homework;

public interface IAiHomeworkAssistant
{
    Task<AiHomeworkGradeResult> GradeSubmissionAsync(
        AiHomeworkGradeSubmissionRequest request,
        CancellationToken cancellationToken = default);

    Task<AiHomeworkSpeakingResult> AnalyzeSpeakingSubmissionAsync(
        AiHomeworkSpeakingSubmissionRequest request,
        CancellationToken cancellationToken = default);

    Task<AiHomeworkSpeakingResult> AnalyzeSpeakingMediaAsync(
        AiHomeworkSpeakingMediaRequest request,
        CancellationToken cancellationToken = default);

    Task<AiQuestionBankGenerationResult> GenerateQuestionBankItemsAsync(
        AiQuestionBankGenerationRequest request,
        CancellationToken cancellationToken = default);

    Task<AiHomeworkHintResult> GetHintAsync(
        AiHomeworkHintRequest request,
        CancellationToken cancellationToken = default);

    Task<AiHomeworkRecommendationResult> GetRecommendationsAsync(
        AiHomeworkRecommendationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class AiHomeworkContext
{
    [JsonPropertyName("homework_id")]
    public string HomeworkId { get; set; } = string.Empty;

    [JsonPropertyName("student_id")]
    public string StudentId { get; set; } = string.Empty;

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = "english";

    [JsonPropertyName("skill")]
    public string Skill { get; set; } = "mixed";

    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("rubric")]
    public string? Rubric { get; set; }

    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    [JsonPropertyName("grammar_tags")]
    public List<string> GrammarTags { get; set; } = new();

    [JsonPropertyName("vocabulary_tags")]
    public List<string> VocabularyTags { get; set; } = new();

    [JsonPropertyName("submission_type")]
    public string? SubmissionType { get; set; }

    [JsonPropertyName("speaking_mode")]
    public string? SpeakingMode { get; set; }

    [JsonPropertyName("target_words")]
    public List<string> TargetWords { get; set; } = new();
}

public sealed class AiHomeworkHintRequest
{
    [JsonPropertyName("context")]
    public AiHomeworkContext Context { get; set; } = new();

    [JsonPropertyName("student_answer_text")]
    public string? StudentAnswerText { get; set; }

    [JsonPropertyName("expected_answer_text")]
    public string? ExpectedAnswerText { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = "vi";
}

public sealed class AiHomeworkGradeSubmissionRequest
{
    public AiHomeworkContext Context { get; set; } = new();
    public string? StudentAnswerText { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? ExpectedAnswerText { get; set; }
    public string Language { get; set; } = "vi";
}

public sealed class AiHomeworkGradeResult
{
    [JsonPropertyName("ai_used")]
    public bool AiUsed { get; set; }

    [JsonPropertyName("result")]
    public AiHomeworkGradePayload Result { get; set; } = new();
}

public sealed class AiHomeworkGradePayload
{
    [JsonPropertyName("score")]
    public float Score { get; set; }

    [JsonPropertyName("max_score")]
    public float MaxScore { get; set; } = 10.0f;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("issues")]
    public List<string> Issues { get; set; } = new();

    [JsonPropertyName("suggestions")]
    public List<string> Suggestions { get; set; } = new();

    [JsonPropertyName("extracted_student_answer")]
    public string? ExtractedStudentAnswer { get; set; }

    [JsonPropertyName("confidence")]
    public Dictionary<string, float> Confidence { get; set; } = new();

    [JsonPropertyName("raw_text")]
    public string? RawText { get; set; }

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();
}

public sealed class AiHomeworkSpeakingSubmissionRequest
{
    public AiHomeworkContext Context { get; set; } = new();
    public string? Transcript { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? ExpectedText { get; set; }
    public string Language { get; set; } = "vi";
}

public sealed class AiHomeworkSpeakingMediaRequest
{
    public AiHomeworkContext Context { get; set; } = new();
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = "speaking-practice";
    public string ContentType { get; set; } = "application/octet-stream";
    public string? ExpectedText { get; set; }
    public string Language { get; set; } = "vi";
}

public sealed class AiHomeworkSpeakingResult
{
    [JsonPropertyName("ai_used")]
    public bool AiUsed { get; set; }

    [JsonPropertyName("result")]
    public AiHomeworkSpeakingPayload Result { get; set; } = new();
}

public sealed class AiHomeworkSpeakingPayload
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("transcript")]
    public string Transcript { get; set; } = string.Empty;

    [JsonPropertyName("overall_score")]
    public float OverallScore { get; set; }

    [JsonPropertyName("pronunciation_score")]
    public float PronunciationScore { get; set; }

    [JsonPropertyName("fluency_score")]
    public float FluencyScore { get; set; }

    [JsonPropertyName("accuracy_score")]
    public float AccuracyScore { get; set; }

    [JsonPropertyName("stars")]
    public int Stars { get; set; }

    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; set; } = new();

    [JsonPropertyName("mispronounced_words")]
    public List<string> MispronouncedWords { get; set; } = new();

    [JsonPropertyName("word_feedback")]
    public List<AiHomeworkSpeakingWordFeedback> WordFeedback { get; set; } = new();

    [JsonPropertyName("phonics_issues")]
    public List<string> PhonicsIssues { get; set; } = new();

    [JsonPropertyName("speaking_issues")]
    public List<string> SpeakingIssues { get; set; } = new();

    [JsonPropertyName("suggestions")]
    public List<string> Suggestions { get; set; } = new();

    [JsonPropertyName("practice_plan")]
    public List<string> PracticePlan { get; set; } = new();

    [JsonPropertyName("confidence")]
    public Dictionary<string, float> Confidence { get; set; } = new();

    [JsonPropertyName("raw_text")]
    public string? RawText { get; set; }

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();
}

public sealed class AiHomeworkSpeakingWordFeedback
{
    [JsonPropertyName("word")]
    public string Word { get; set; } = string.Empty;

    [JsonPropertyName("heard_as")]
    public string? HeardAs { get; set; }

    [JsonPropertyName("issue")]
    public string Issue { get; set; } = string.Empty;

    [JsonPropertyName("tip")]
    public string Tip { get; set; } = string.Empty;
}

public sealed class AiQuestionBankGenerationRequest
{
    [JsonPropertyName("program_id")]
    public string ProgramId { get; set; } = string.Empty;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("question_type")]
    public string QuestionType { get; set; } = "MultipleChoice";

    [JsonPropertyName("question_count")]
    public int QuestionCount { get; set; } = 5;

    [JsonPropertyName("level")]
    public string Level { get; set; } = "Medium";

    [JsonPropertyName("skill")]
    public string? Skill { get; set; }

    [JsonPropertyName("task_style")]
    public string TaskStyle { get; set; } = "standard";

    [JsonPropertyName("grammar_tags")]
    public List<string> GrammarTags { get; set; } = new();

    [JsonPropertyName("vocabulary_tags")]
    public List<string> VocabularyTags { get; set; } = new();

    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = "vi";

    [JsonPropertyName("points_per_question")]
    public int PointsPerQuestion { get; set; } = 1;
}

public sealed class AiQuestionBankGenerationResult
{
    [JsonPropertyName("ai_used")]
    public bool AiUsed { get; set; }

    [JsonPropertyName("result")]
    public AiQuestionBankGenerationPayload Result { get; set; } = new();
}

public sealed class AiQuestionBankGenerationPayload
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("items")]
    public List<AiGeneratedQuestionBankItem> Items { get; set; } = new();

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();
}

public sealed class AiGeneratedQuestionBankItem
{
    [JsonPropertyName("question_text")]
    public string QuestionText { get; set; } = string.Empty;

    [JsonPropertyName("question_type")]
    public string QuestionType { get; set; } = "MultipleChoice";

    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = new();

    [JsonPropertyName("correct_answer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [JsonPropertyName("points")]
    public int Points { get; set; } = 1;

    [JsonPropertyName("explanation")]
    public string? Explanation { get; set; }

    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    [JsonPropertyName("skill")]
    public string? Skill { get; set; }

    [JsonPropertyName("grammar_tags")]
    public List<string> GrammarTags { get; set; } = new();

    [JsonPropertyName("vocabulary_tags")]
    public List<string> VocabularyTags { get; set; } = new();

    [JsonPropertyName("level")]
    public string Level { get; set; } = "Medium";
}

public sealed class AiHomeworkHintResult
{
    [JsonPropertyName("ai_used")]
    public bool AiUsed { get; set; }

    [JsonPropertyName("result")]
    public AiHomeworkHintPayload Result { get; set; } = new();
}

public sealed class AiHomeworkHintPayload
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("hints")]
    public List<string> Hints { get; set; } = new();

    [JsonPropertyName("grammar_focus")]
    public List<string> GrammarFocus { get; set; } = new();

    [JsonPropertyName("vocabulary_focus")]
    public List<string> VocabularyFocus { get; set; } = new();

    [JsonPropertyName("encouragement")]
    public string Encouragement { get; set; } = string.Empty;

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();
}

public sealed class AiHomeworkRecommendationRequest
{
    [JsonPropertyName("context")]
    public AiHomeworkContext Context { get; set; } = new();

    [JsonPropertyName("latest_score")]
    public float? LatestScore { get; set; }

    [JsonPropertyName("max_score")]
    public float? MaxScore { get; set; }

    [JsonPropertyName("teacher_feedback")]
    public string? TeacherFeedback { get; set; }

    [JsonPropertyName("ai_feedback")]
    public string? AiFeedback { get; set; }

    [JsonPropertyName("student_answer_text")]
    public string? StudentAnswerText { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = "vi";
}

public sealed class AiHomeworkRecommendationResult
{
    [JsonPropertyName("ai_used")]
    public bool AiUsed { get; set; }

    [JsonPropertyName("result")]
    public AiHomeworkRecommendationPayload Result { get; set; } = new();
}

public sealed class AiHomeworkRecommendationPayload
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("focus_skill")]
    public string FocusSkill { get; set; } = string.Empty;

    [JsonPropertyName("topics")]
    public List<string> Topics { get; set; } = new();

    [JsonPropertyName("grammar_tags")]
    public List<string> GrammarTags { get; set; } = new();

    [JsonPropertyName("vocabulary_tags")]
    public List<string> VocabularyTags { get; set; } = new();

    [JsonPropertyName("recommended_levels")]
    public List<string> RecommendedLevels { get; set; } = new();

    [JsonPropertyName("practice_types")]
    public List<string> PracticeTypes { get; set; } = new();

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();
}
