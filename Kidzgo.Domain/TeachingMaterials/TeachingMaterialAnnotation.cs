using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.TeachingMaterials;

public class TeachingMaterialAnnotation : Entity
{
    public Guid Id { get; set; }
    public Guid TeachingMaterialId { get; set; }
    public int? SlideNumber { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = null!;
    public string? Color { get; set; } = "#FFD700";
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }
    public TeachingMaterialAnnotationType Type { get; set; } = TeachingMaterialAnnotationType.Note;
    public TeachingMaterialAnnotationVisibility Visibility { get; set; } = TeachingMaterialAnnotationVisibility.Private;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TeachingMaterial TeachingMaterial { get; set; } = null!;
    public User User { get; set; } = null!;
}
