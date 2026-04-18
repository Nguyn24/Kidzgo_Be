using System.Text.Json.Serialization;
using Kidzgo.API.Extensions;

namespace Kidzgo.API.Requests;

public sealed class UpdatePlacementTestResultsRequest
{
    public decimal? ListeningScore { get; set; }
    public decimal? SpeakingScore { get; set; }
    public decimal? ReadingScore { get; set; }
    public decimal? WritingScore { get; set; }
    public decimal? ResultScore { get; set; }
    /// <summary>
    /// Recommended primary program id. Send Guid.Empty to clear the current recommendation.
    /// </summary>
    public Guid? ProgramRecommendationId { get; set; }

    /// <summary>
    /// Recommended secondary program id. Send Guid.Empty to clear the current secondary recommendation.
    /// </summary>
    public Guid? SecondaryProgramRecommendationId { get; set; }

    public string? SecondaryProgramSkillFocus { get; set; }

    /// <summary>
    /// Accepts a single URL string for backward compatibility or an array of URL strings.
    /// </summary>
    [JsonConverter(typeof(StringOrStringArrayJsonConverter))]
    public List<string>? AttachmentUrl { get; set; }
}

