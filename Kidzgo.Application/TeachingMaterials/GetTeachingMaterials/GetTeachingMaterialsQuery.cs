using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterials;

public sealed class GetTeachingMaterialsQuery : IQuery<GetTeachingMaterialsResponse>, IPageableQuery
{
    public Guid? ProgramId { get; init; }
    public int? UnitNumber { get; init; }
    public int? LessonNumber { get; init; }
    public string? FileType { get; init; }
    public string? Category { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
