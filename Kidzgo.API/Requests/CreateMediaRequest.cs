using Kidzgo.Domain.Media;

namespace Kidzgo.API.Requests;

public sealed record CreateMediaRequest
{
    public Guid BranchId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public string? MonthTag { get; init; }
    public MediaType Type { get; init; }
    public MediaContentType ContentType { get; init; }
    public string Url { get; init; } = null!;
    public string? MimeType { get; init; }
    public long FileSize { get; init; }
    public string? OriginalFileName { get; init; }
    public int? DisplayOrder { get; init; }
    public MediaOwnershipScope OwnershipScope { get; init; }
    public string? Caption { get; init; }
    public Visibility Visibility { get; init; }
}

