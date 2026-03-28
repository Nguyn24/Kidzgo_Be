using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.GetLessonBundle;

public sealed class GetLessonBundleQuery : IQuery<GetLessonBundleResponse>
{
    public Guid ProgramId { get; init; }
    public int UnitNumber { get; init; }
    public int LessonNumber { get; init; }
}
