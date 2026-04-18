using System.Text.Json;

namespace Kidzgo.Application.PlacementTests.Shared;

internal static class PlacementTestAttachmentUrlHelper
{
    public static IReadOnlyList<string> Parse(string? storedValue)
    {
        if (string.IsNullOrWhiteSpace(storedValue))
        {
            return Array.Empty<string>();
        }

        var trimmedValue = storedValue.Trim();

        if (trimmedValue.StartsWith("[", StringComparison.Ordinal))
        {
            try
            {
                var urls = JsonSerializer.Deserialize<List<string?>>(trimmedValue);
                return Normalize(urls);
            }
            catch (JsonException)
            {
                // Older records may contain a plain URL-like string that starts with '['.
            }
        }

        return Normalize(new[] { trimmedValue });
    }

    public static string? Serialize(IEnumerable<string>? urls)
    {
        var normalizedUrls = Normalize(urls);
        return normalizedUrls.Count == 0
            ? null
            : JsonSerializer.Serialize(normalizedUrls);
    }

    private static IReadOnlyList<string> Normalize(IEnumerable<string?>? urls)
    {
        if (urls is null)
        {
            return Array.Empty<string>();
        }

        return urls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => url!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
