namespace Kidzgo.API.Requests;

public sealed class UpdateTeachingMaterialViewProgressRequest
{
    public int ProgressPercent { get; init; }
    public int? LastSlideViewed { get; init; }
    public int TotalTimeSeconds { get; init; }
}
