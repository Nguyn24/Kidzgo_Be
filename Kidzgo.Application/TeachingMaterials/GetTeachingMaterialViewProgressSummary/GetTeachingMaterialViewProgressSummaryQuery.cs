using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialViewProgressSummary;

public sealed class GetTeachingMaterialViewProgressSummaryQuery : IQuery<GetTeachingMaterialViewProgressSummaryResponse>
{
    public Guid TeachingMaterialId { get; init; }
}
