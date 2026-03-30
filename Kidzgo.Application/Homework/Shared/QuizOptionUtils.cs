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
                if (!item.TryGetProperty("questionId", out var questionIdProp) ||
                    questionIdProp.ValueKind != JsonValueKind.String ||
                    !Guid.TryParse(questionIdProp.GetString(), out var questionId))
                {
                    continue;
                }

                if (item.TryGetProperty("selectedOptionId", out var selectedProp))
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
                else if (item.TryGetProperty("answer", out var answerProp) &&
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
}
