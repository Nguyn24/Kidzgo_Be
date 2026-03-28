using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialById;

public sealed class GetTeachingMaterialByIdQuery : IQuery<GetTeachingMaterialByIdResponse>
{
    public Guid TeachingMaterialId { get; init; }
}
