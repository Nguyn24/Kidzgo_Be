using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Kidzgo.Application.Homework.Shared;

internal static class QuizOptionUtils
{
    public static List<string> ParseOptions(string? optionsJson)
    {
        if (string.IsNullOrWhiteSpace(optionsJson))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(optionsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public static bool TryResolveCorrectAnswer(
        IReadOnlyList<string> options,
        string? rawCorrectAnswer,
        out int index,
        out string? optionText)
    {
        index = -1;
        optionText = null;

        if (options.Count == 0 || string.IsNullOrWhiteSpace(rawCorrectAnswer))
        {
            return false;
        }

        var trimmed = rawCorrectAnswer.Trim();

        if (int.TryParse(trimmed, out var numeric))
        {
            if (numeric >= 0 && numeric < options.Count)
            {
                index = numeric;
                optionText = options[index];
                return true;
            }

            if (numeric >= 1 && numeric <= options.Count)
            {
                index = numeric - 1;
                optionText = options[index];
                return true;
            }
        }

        if (trimmed.Length == 1)
        {
            var ch = char.ToUpperInvariant(trimmed[0]);
            if (ch >= 'A' && ch < 'A' + options.Count)
            {
                index = ch - 'A';
                optionText = options[index];
                return true;
            }
        }

        var exactMatchIndex = FindOptionIndex(options, trimmed, StringComparison.Ordinal);
        if (exactMatchIndex >= 0)
        {
            index = exactMatchIndex;
            optionText = options[index];
            return true;
        }

        var ignoreCaseMatchIndex = FindOptionIndex(options, trimmed, StringComparison.OrdinalIgnoreCase);
        if (ignoreCaseMatchIndex >= 0)
        {
            index = ignoreCaseMatchIndex;
            optionText = options[index];
            return true;
        }

        return false;
    }

    public static string? NormalizeCorrectAnswerForStorage(
        IReadOnlyList<string> options,
        string? rawCorrectAnswer)
    {
        return TryResolveCorrectAnswer(options, rawCorrectAnswer, out _, out var optionText)
            ? optionText
            : null;
    }

    public static bool TryBuildCorrectOption(
        Guid questionId,
        IReadOnlyList<string> options,
        string? rawCorrectAnswer,
        out Guid? optionId,
        out string? optionText)
    {
        optionId = null;
        optionText = null;

        if (!TryResolveCorrectAnswer(options, rawCorrectAnswer, out var index, out optionText))
        {
            return false;
        }

        optionId = BuildOptionId(questionId, index);
        return true;
    }

    public static Guid BuildOptionId(Guid questionId, int orderIndex)
    {
        var input = $"{questionId:N}:{orderIndex}";
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }

    public static Dictionary<Guid, Guid?> ParseSelectedOptions(string? textAnswer)
    {
        var result = new Dictionary<Guid, Guid?>();
        if (string.IsNullOrWhiteSpace(textAnswer))
        {
            return result;
        }

        try
        {
            using var doc = JsonDocument.Parse(textAnswer);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                return result;
            }

            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (!TryGetPropertyIgnoreCase(item, "questionId", out var questionIdProp) ||
                    questionIdProp.ValueKind != JsonValueKind.String ||
                    !Guid.TryParse(questionIdProp.GetString(), out var questionId))
                {
                    continue;
                }

                if (TryGetPropertyIgnoreCase(item, "selectedOptionId", out var selectedProp))
                {
                    if (selectedProp.ValueKind == JsonValueKind.String &&
                        Guid.TryParse(selectedProp.GetString(), out var selectedId))
                    {
                        result[questionId] = selectedId;
                    }
                    else
                    {
                        result[questionId] = null;
                    }
                }
                else if (TryGetPropertyIgnoreCase(item, "answer", out var answerProp) &&
                         answerProp.ValueKind == JsonValueKind.String &&
                         Guid.TryParse(answerProp.GetString(), out var legacySelectedId))
                {
                    result[questionId] = legacySelectedId;
                }
            }
        }
        catch
        {
            return result;
        }

        return result;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.TryGetProperty(propertyName, out value))
        {
            return true;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static int FindOptionIndex(
        IReadOnlyList<string> options,
        string candidate,
        StringComparison comparison)
    {
        var matchIndex = -1;

        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i]?.Trim() ?? string.Empty;
            if (!string.Equals(option, candidate, comparison))
            {
                continue;
            }

            if (matchIndex >= 0)
            {
                return -1;
            }

            matchIndex = i;
        }

        return matchIndex;
    }
}
