using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.DeleteTeachingMaterialBookmark;

public sealed class DeleteTeachingMaterialBookmarkCommand : ICommand
{
    public Guid TeachingMaterialId { get; init; }
}
