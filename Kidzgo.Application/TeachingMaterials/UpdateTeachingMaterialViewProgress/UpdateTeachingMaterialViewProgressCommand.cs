using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TeachingMaterials.UpdateTeachingMaterialViewProgress;

public sealed class UpdateTeachingMaterialViewProgressCommand : ICommand<TeachingMaterialViewProgressResponse>
{
    public Guid TeachingMaterialId { get; init; }
    public int ProgressPercent { get; init; }
    public int? LastSlideViewed { get; init; }
    public int TotalTimeSeconds { get; init; }
}
