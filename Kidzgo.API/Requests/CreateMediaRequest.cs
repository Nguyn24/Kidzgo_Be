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
    public string? Caption { get; init; }
    public Visibility Visibility { get; init; }
}

