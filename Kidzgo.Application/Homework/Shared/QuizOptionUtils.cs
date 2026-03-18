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
}
