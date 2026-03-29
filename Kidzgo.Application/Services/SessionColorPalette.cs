namespace Kidzgo.Application.Services;

public static class SessionColorPalette
{
    private static readonly string[] Colors =
    [
        "#FADADD",
        "#FBCFE8",
        "#F9D5E5",
        "#FECDD3",
        "#FFE4E6",
        "#FDE2B8",
        "#FED7AA",
        "#FCD5B5",
        "#FFE5B4",
        "#FFE8CC",
        "#FFF1A8",
        "#FEF3C7",
        "#FFF4B8",
        "#FFF7CC",
        "#FAF3DD",
        "#DDF4DE",
        "#DCFCE7",
        "#D9F99D",
        "#ECFCCB",
        "#E6F7D4",
        "#D7F0F7",
        "#CFFAFE",
        "#C7F9F2",
        "#D6F5F3",
        "#E0F7FA",
        "#DCEBFF",
        "#DBEAFE",
        "#BFDBFE",
        "#D6E4FF",
        "#E0ECFF",
        "#E6D9FF",
        "#E9D5FF",
        "#DDD6FE",
        "#EDE9FE",
        "#F3E8FF",
        "#F8D8F0",
        "#F5D0FE",
        "#FCE7F3",
        "#F9E2F4",
        "#FDE2FF",
        "#FFDCCF",
        "#FFDDD6",
        "#FEE2E2",
        "#FFE4D6",
        "#FFF0E5",
        "#E4F7C5",
        "#F0F9C8",
        "#EEF7B9",
        "#F3FAD8",
        "#EAF7D8"
    ];

    public static string GetRandomColor()
    {
        return Colors[Random.Shared.Next(Colors.Length)];
    }
}
