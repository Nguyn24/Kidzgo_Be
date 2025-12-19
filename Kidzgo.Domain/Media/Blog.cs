using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Media;

public class Blog : Entity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Summary { get; set; } // Short summary for preview
    public string Content { get; set; } = null!; // Full blog content (can be HTML/Markdown)
    public string? FeaturedImageUrl { get; set; } // Featured image URL
    public Guid CreatedBy { get; set; } // Admin/Staff who created
    public bool IsPublished { get; set; } // Whether published on landing page
    public bool IsDeleted { get; set; }
    public DateTime? PublishedAt { get; set; } // When published
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User CreatedByUser { get; set; } = null!;
}

