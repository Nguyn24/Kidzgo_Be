using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.GetLessonMaterialProgress;

public sealed class GetLessonMaterialProgressQuery : IQuery<GetLessonMaterialProgressResponse>
{
    public Guid ProgramId { get; init; }
    public int UnitNumber { get; init; }
    public int LessonNumber { get; init; }
}
