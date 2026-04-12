namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlideNotes;

public sealed class GetTeachingMaterialSlideNotesResponse
{
    public Guid MaterialId { get; init; }
    public int SlideNumber { get; init; }
    public bool HasNotes { get; init; }
    public string? Notes { get; init; }
}
