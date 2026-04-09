using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialAnnotation;

namespace Kidzgo.Application.TeachingMaterials.UpdateTeachingMaterialAnnotation;

public sealed class UpdateTeachingMaterialAnnotationCommand : ICommand<TeachingMaterialAnnotationResponse>
{
    public Guid AnnotationId { get; init; }
    public string Content { get; init; } = null!;
    public string? Color { get; init; }
    public double? PositionX { get; init; }
    public double? PositionY { get; init; }
    public string Type { get; init; } = "Note";
    public string Visibility { get; init; } = "Private";
}
