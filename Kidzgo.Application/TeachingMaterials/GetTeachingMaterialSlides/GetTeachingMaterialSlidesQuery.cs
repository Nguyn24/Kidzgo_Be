using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlides;

public sealed class GetTeachingMaterialSlidesQuery : IQuery<GetTeachingMaterialSlidesResponse>
{
    public Guid TeachingMaterialId { get; init; }
}
