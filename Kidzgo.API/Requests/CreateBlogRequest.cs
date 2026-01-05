namespace Kidzgo.API.Requests;

public sealed class CreateBlogRequest
{
    public string Title { get; set; } = null!;
    public string? Summary { get; set; }
    public string Content { get; set; } = null!;
    public string? FeaturedImageUrl { get; set; }
}

