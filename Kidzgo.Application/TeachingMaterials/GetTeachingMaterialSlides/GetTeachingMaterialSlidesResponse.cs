namespace Kidzgo.Application.TeachingMaterials.GetTeachingMaterialSlides;

public sealed class GetTeachingMaterialSlidesResponse
{
    public Guid MaterialId { get; init; }
    public string DisplayName { get; init; } = null!;
    public int TotalSlides { get; init; }
    public IReadOnlyCollection<TeachingMaterialSlideDto> Slides { get; init; } = [];
}

public sealed class TeachingMaterialSlideDto
{
    public int SlideNumber { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string PreviewUrl { get; init; } = null!;
    public string ThumbnailUrl { get; init; } = null!;
    public bool HasNotes { get; init; }
}
