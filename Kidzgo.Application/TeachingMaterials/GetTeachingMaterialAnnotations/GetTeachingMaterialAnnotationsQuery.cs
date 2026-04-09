using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.CreateTeachingMaterialAnnotation;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialAnnotations;

public sealed class GetTeachingMaterialAnnotationsQuery : IQuery<IReadOnlyCollection<TeachingMaterialAnnotationResponse>>
{
    public Guid TeachingMaterialId { get; init; }
    public int? SlideNumber { get; init; }
    public string Visibility { get; init; } = "All";
}
