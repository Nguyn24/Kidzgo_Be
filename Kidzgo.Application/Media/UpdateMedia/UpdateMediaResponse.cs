using Kidzgo.Domain.Media;

namespace Kidzgo.Application.Media.UpdateMedia;

public sealed record UpdateMediaResponse
{
    public Guid Id { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public string? MonthTag { get; init; }
    public MediaContentType ContentType { get; init; }
    public string? Caption { get; init; }
    public Visibility Visibility { get; init; }
    public DateTime UpdatedAt { get; init; }
}

