using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialPreviewPdf;

public sealed class GetTeachingMaterialPreviewPdfQuery : IQuery<GetTeachingMaterialPreviewPdfResponse>
{
    public Guid TeachingMaterialId { get; init; }
}
