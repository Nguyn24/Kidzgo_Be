using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.DownloadTeachingMaterial;

public sealed class DownloadTeachingMaterialQuery : IQuery<DownloadTeachingMaterialResponse>
{
    public Guid TeachingMaterialId { get; init; }
}
