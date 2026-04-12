namespace Kidzgo.Application.Homework.Shared;

public static class HomeworkDeliveryMetadata
{
    private static readonly string[] ListeningSkillKeywords =
    [
        "listening",
        "nghe"
    ];

    private static readonly string[] AudioExtensions =
    [
        ".mp3",
        ".wav",
        ".m4a",
        ".aac",
        ".ogg",
        ".flac",
        ".wma"
    ];

    public static bool IsListeningQuiz(string? skills, string? attachmentUrl)
    {
        return HasListeningSkill(skills) || HasAudioAttachment(attachmentUrl);
    }

    public static string? NormalizeSkills(string? skills, string? attachmentUrl)
    {
        var normalizedSkills = string.IsNullOrWhiteSpace(skills)
            ? null
            : skills.Trim();

        if (!string.IsNullOrWhiteSpace(normalizedSkills))
        {
            return normalizedSkills;
        }

        return HasAudioAttachment(attachmentUrl) ? "Listening" : null;
    }

    public static bool HasListeningSkill(string? skills)
    {
        if (string.IsNullOrWhiteSpace(skills))
        {
            return false;
        }

        return ListeningSkillKeywords.Any(keyword =>
            skills.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    public static bool HasAudioAttachment(string? attachmentUrl)
    {
        if (string.IsNullOrWhiteSpace(attachmentUrl))
        {
            return false;
        }

        return AudioExtensions.Any(extension =>
            attachmentUrl.Contains(extension, StringComparison.OrdinalIgnoreCase));
    }
}
