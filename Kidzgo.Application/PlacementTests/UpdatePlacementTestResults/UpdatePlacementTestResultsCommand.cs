using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.UpdatePlacementTestResults;

public sealed class UpdatePlacementTestResultsCommand : ICommand<UpdatePlacementTestResultsResponse>
{
    public Guid PlacementTestId { get; init; }
    public decimal? ListeningScore { get; init; }
    public decimal? SpeakingScore { get; init; }
    public decimal? ReadingScore { get; init; }
    public decimal? WritingScore { get; init; }
    public decimal? ResultScore { get; init; }
    public string? LevelRecommendation { get; init; }
    public string? ProgramRecommendation { get; init; }
    public string? AttachmentUrl { get; init; }
}

