using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlideImage;

public sealed class GetTeachingMaterialSlideImageQuery : IQuery<GetTeachingMaterialSlideImageResponse>
{
    public Guid TeachingMaterialId { get; init; }
    public int SlideNumber { get; init; }
    public TeachingMaterialSlideImageKind ImageKind { get; init; }
}

public enum TeachingMaterialSlideImageKind
{
    Preview,
    Thumbnail
}
