using Kidzgo.Domain.Notifications;

namespace Kidzgo.Application.NotificationTemplates;

internal static class NotificationTemplateContractMapper
{
    public static string InferCategory(string code, NotificationChannel channel)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Contains("HOMEWORK"))
        {
            return "Homework";
        }

        if (normalizedCode.Contains("SESSION") || normalizedCode.Contains("MAKEUP"))
        {
            return "Session";
        }

        if (normalizedCode.Contains("TUITION") || normalizedCode.Contains("INVOICE") || normalizedCode.Contains("PAY"))
        {
            return "Finance";
        }

        if (normalizedCode.Contains("MEDIA"))
        {
            return "Media";
        }

        if (normalizedCode.Contains("MISSION") || normalizedCode.Contains("REWARD") || normalizedCode.Contains("STAR"))
        {
            return "Gamification";
        }

        if (normalizedCode.Contains("PAUSE_ENROLLMENT") || normalizedCode.Contains("ENROLLMENT"))
        {
            return "Enrollment";
        }

        if (normalizedCode.Contains("PROFILE") || normalizedCode.Contains("ACCOUNT") || normalizedCode.Contains("PASSWORD") || normalizedCode.Contains("PIN"))
        {
            return "Account";
        }

        return channel.ToString();
    }
}
