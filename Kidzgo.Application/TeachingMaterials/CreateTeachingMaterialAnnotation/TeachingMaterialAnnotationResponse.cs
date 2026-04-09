namespace Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialAnnotation;

public sealed class TeachingMaterialAnnotationResponse
{
    public Guid Id { get; init; }
    public int? SlideNumber { get; init; }
    public string Content { get; init; } = null!;
    public string? Color { get; init; }
    public double? PositionX { get; init; }
    public double? PositionY { get; init; }
    public string Type { get; init; } = null!;
    public string Visibility { get; init; } = null!;
    public Guid CreatedByUserId { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
