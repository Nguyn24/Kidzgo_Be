namespace Kidzgo.API.Requests;

public sealed class UpdatePlacementTestResultsRequest
{
    public decimal? ListeningScore { get; set; }
    public decimal? SpeakingScore { get; set; }
    public decimal? ReadingScore { get; set; }
    public decimal? WritingScore { get; set; }
    public decimal? ResultScore { get; set; }
    public string? ProgramRecommendation { get; set; }
    public string? SecondaryProgramRecommendation { get; set; }
    public bool? IsSecondaryProgramSupplementary { get; set; }
    public string? SecondaryProgramSkillFocus { get; set; }
    public string? AttachmentUrl { get; set; }
}

