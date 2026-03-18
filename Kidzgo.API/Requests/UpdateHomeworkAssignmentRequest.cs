namespace Kidzgo.API.Requests;

public sealed class UpdateHomeworkAssignmentRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime? DueAt { get; init; }
    public string? Book { get; init; }
    public string? Pages { get; init; }
    public string? Skills { get; init; }
    public string? SubmissionType { get; init; }
    public decimal? MaxScore { get; init; }
    public int? RewardStars { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public bool? AllowResubmit { get; init; }
    public Guid? MissionId { get; init; }
    public string? Instructions { get; init; }
    public string? ExpectedAnswer { get; init; }
    public string? Rubric { get; init; }
}

