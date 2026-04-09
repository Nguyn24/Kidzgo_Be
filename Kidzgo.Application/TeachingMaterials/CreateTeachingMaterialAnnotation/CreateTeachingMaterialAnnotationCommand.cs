using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialAnnotation;

public sealed class CreateTeachingMaterialAnnotationCommand : ICommand<TeachingMaterialAnnotationResponse>
{
    public Guid TeachingMaterialId { get; init; }
    public int? SlideNumber { get; init; }
    public string Content { get; init; } = null!;
    public string? Color { get; init; }
    public double? PositionX { get; init; }
    public double? PositionY { get; init; }
    public string Type { get; init; } = "Note";
    public string Visibility { get; init; } = "Private";
}
