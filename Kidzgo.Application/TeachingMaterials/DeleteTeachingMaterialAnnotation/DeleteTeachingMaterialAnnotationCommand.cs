using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.DeleteTeachingMaterialAnnotation;

public sealed class DeleteTeachingMaterialAnnotationCommand : ICommand
{
    public Guid AnnotationId { get; init; }
}
