namespace Kidzgo.Application.Media.PublishMedia;

public sealed record PublishMediaResponse
{
    public Guid Id { get; init; }
    public bool IsPublished { get; init; }
    public DateTime UpdatedAt { get; init; }
}

