using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TeachingMaterials.UpdateTeachingMaterialViewProgress;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialViewProgress;

public sealed class GetTeachingMaterialViewProgressQuery : IQuery<TeachingMaterialViewProgressResponse>
{
    public Guid TeachingMaterialId { get; init; }
}
