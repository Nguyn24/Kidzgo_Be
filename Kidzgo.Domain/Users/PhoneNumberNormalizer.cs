using System.Text;

namespace Kidzgo.Domain.Users;

public static class PhoneNumberNormalizer
{
    public static string? NormalizeVietnamesePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        var trimmedPhoneNumber = phoneNumber.Trim();
        var digitsBuilder = new StringBuilder(trimmedPhoneNumber.Length);

        foreach (var character in trimmedPhoneNumber)
        {
            if (char.IsDigit(character))
            {
                digitsBuilder.Append(character);
                continue;
            }

            if (character is ' ' or '-' or '.' or '(' or ')' or '+')
            {
                continue;
            }

            return null;
        }

        if (digitsBuilder.Length == 0)
        {
            return null;
        }

        var digits = digitsBuilder.ToString();

        if (digits.StartsWith("84", StringComparison.Ordinal) &&
            !digits.StartsWith("0", StringComparison.Ordinal))
        {
            digits = "0" + digits[2..];
        }

        return IsValidLocalVietnamesePhoneNumber(digits) ? digits : null;
    }

    public static bool IsValidVietnamesePhoneNumber(string? phoneNumber)
    {
        return NormalizeVietnamesePhoneNumber(phoneNumber) is not null;
    }

    public static string[] GetLookupCandidates(string? phoneNumber)
    {
        var normalizedPhoneNumber = NormalizeVietnamesePhoneNumber(phoneNumber);

        if (normalizedPhoneNumber is null)
        {
            return [];
        }

        return
        [
            normalizedPhoneNumber,
            "84" + normalizedPhoneNumber[1..]
        ];
    }

    private static bool IsValidLocalVietnamesePhoneNumber(string phoneNumber)
    {
        return phoneNumber.Length is 10 or 11 &&
               phoneNumber.StartsWith("0", StringComparison.Ordinal);
    }
}
