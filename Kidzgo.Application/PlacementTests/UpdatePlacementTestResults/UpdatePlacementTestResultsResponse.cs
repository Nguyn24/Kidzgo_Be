namespace Kidzgo.Application.PlacementTests.UpdatePlacementTestResults;

public sealed class UpdatePlacementTestResultsResponse
{
    public Guid Id { get; init; }
    public decimal? ListeningScore { get; init; }
    public decimal? SpeakingScore { get; init; }
    public decimal? ReadingScore { get; init; }
    public decimal? WritingScore { get; init; }
    public decimal? ResultScore { get; init; }
    public Guid? ProgramRecommendationId { get; init; }
    public string? ProgramRecommendationName { get; init; }
    public Guid? SecondaryProgramRecommendationId { get; init; }
    public string? SecondaryProgramRecommendationName { get; init; }
    public string? SecondaryProgramSkillFocus { get; init; }
    public string? AttachmentUrl { get; init; }
    public IReadOnlyList<string> AttachmentUrls { get; init; } = Array.Empty<string>();
    public string Status { get; init; } = null!;
    public DateTime UpdatedAt { get; init; }
    public Guid? NewRegistrationId { get; init; }
}

