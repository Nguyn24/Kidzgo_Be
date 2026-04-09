using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.TeachingMaterials;

public class TeachingMaterialSlide : Entity
{
    public Guid Id { get; set; }
    public Guid TeachingMaterialId { get; set; }
    public int SlideNumber { get; set; }
    public string PreviewImagePath { get; set; } = null!;
    public string ThumbnailImagePath { get; set; } = null!;
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public string? Notes { get; set; }
    public DateTime GeneratedAt { get; set; }

    public TeachingMaterial TeachingMaterial { get; set; } = null!;
}
