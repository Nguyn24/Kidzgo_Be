namespace Kidzgo.Application.PlacementTests.UpdatePlacementTestResults;

public sealed class UpdatePlacementTestResultsResponse
{
    public Guid Id { get; init; }
    public decimal? ListeningScore { get; init; }
    public decimal? SpeakingScore { get; init; }
    public decimal? ReadingScore { get; init; }
    public decimal? WritingScore { get; init; }
    public decimal? ResultScore { get; init; }
    public string? LevelRecommendation { get; init; }
    public string? ProgramRecommendation { get; init; }
    public string? AttachmentUrl { get; init; }
    public string Status { get; init; } = null!;
    public DateTime UpdatedAt { get; init; }
}

