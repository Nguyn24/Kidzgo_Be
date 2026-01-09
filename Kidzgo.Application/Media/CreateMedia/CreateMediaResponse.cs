using Kidzgo.Domain.Media;

namespace Kidzgo.Application.Media.CreateMedia;

public sealed record CreateMediaResponse
{
    public Guid Id { get; init; }
    public Guid UploaderId { get; init; }
    public string UploaderName { get; init; } = null!;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid? ClassId { get; init; }
    public string? ClassName { get; init; }
    public Guid? StudentProfileId { get; init; }
    public string? StudentName { get; init; }
    public string? MonthTag { get; init; }
    public MediaType Type { get; init; }
    public MediaContentType ContentType { get; init; }
    public string Url { get; init; } = null!;
    public string? Caption { get; init; }
    public Visibility Visibility { get; init; }
    public ApprovalStatus ApprovalStatus { get; init; }
    public bool IsPublished { get; init; }
    public DateTime CreatedAt { get; init; }
}

