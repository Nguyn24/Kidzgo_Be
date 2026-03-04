namespace Kidzgo.API.Requests;

public sealed class UpdateBlogRequest
{
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string Content { get; set; } = null!;
    public string? FeaturedImageUrl { get; set; }
    public string? AttachmentImageUrl { get; set; }
    public string? AttachmentFileUrl { get; set; }
}

