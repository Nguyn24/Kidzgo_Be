using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.TeachingMaterials;

public class TeachingMaterialBookmark : Entity
{
    public Guid Id { get; set; }
    public Guid TeachingMaterialId { get; set; }
    public Guid UserId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public TeachingMaterial TeachingMaterial { get; set; } = null!;
    public User User { get; set; } = null!;
}
