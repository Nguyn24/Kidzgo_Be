using System.Globalization;

namespace Kidzgo.Application.Registrations.Shared;

internal static class EnrollmentConfirmationPaymentQrBuilder
{
    public static string? BuildVietQrUrl(
        string? bankBin,
        string? bankCode,
        string? accountNumber,
        string? accountName,
        string? transferContent,
        decimal? amount,
        string? template)
    {
        var bankIdentifier = FirstNonEmpty(bankBin, bankCode);
        if (string.IsNullOrWhiteSpace(bankIdentifier) || string.IsNullOrWhiteSpace(accountNumber))
        {
            return null;
        }

        var effectiveTemplate = string.IsNullOrWhiteSpace(template) ? "compact2" : template.Trim();
        var url = $"https://img.vietqr.io/image/{EscapePath(bankIdentifier)}-{EscapePath(accountNumber)}-{EscapePath(effectiveTemplate)}.png";

        var query = new List<string>();
        if (amount.HasValue && amount.Value > 0)
        {
            query.Add($"amount={decimal.ToInt64(decimal.Truncate(amount.Value)).ToString(CultureInfo.InvariantCulture)}");
        }

        if (!string.IsNullOrWhiteSpace(transferContent))
        {
            query.Add($"addInfo={Uri.EscapeDataString(transferContent.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(accountName))
        {
            query.Add($"accountName={Uri.EscapeDataString(accountName.Trim())}");
        }

        return query.Count == 0 ? url : $"{url}?{string.Join("&", query)}";
    }

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();

    private static string EscapePath(string value)
        => Uri.EscapeDataString(value.Trim());
}
