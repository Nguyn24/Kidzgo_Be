using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlideNotes;

public sealed class GetTeachingMaterialSlideNotesQuery : IQuery<GetTeachingMaterialSlideNotesResponse>
{
    public Guid TeachingMaterialId { get; init; }
    public int SlideNumber { get; init; }
}
