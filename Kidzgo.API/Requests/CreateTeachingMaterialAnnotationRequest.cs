namespace Kidzgo.API.Requests;

public sealed class CreateTeachingMaterialAnnotationRequest
{
    public int? SlideNumber { get; init; }
    public string Content { get; init; } = null!;
    public string? Color { get; init; }
    public double? PositionX { get; init; }
    public double? PositionY { get; init; }
    public string Type { get; init; } = "Note";
    public string Visibility { get; init; } = "Private";
}
