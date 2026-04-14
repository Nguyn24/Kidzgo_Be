namespace Kidzgo.Application.IncidentReports.Shared;

public class IncidentReportDto
{
    public Guid Id { get; init; }
    public Guid OpenedByUserId { get; init; }
    public string OpenedByUserName { get; init; } = null!;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid? ClassId { get; init; }
    public string? ClassCode { get; init; }
    public string? ClassTitle { get; init; }
    public string Category { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string Status { get; init; } = null!;
    public Guid? AssignedToUserId { get; init; }
    public string? AssignedToUserName { get; init; }
    public string? EvidenceUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public int CommentCount { get; init; }
}

public sealed class IncidentReportCommentDto
{
    public Guid Id { get; init; }
    public Guid CommenterUserId { get; init; }
    public string CommenterUserName { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? AttachmentUrl { get; init; }
    public string? CommentType { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class IncidentReportDetailDto : IncidentReportDto
{
    public List<IncidentReportCommentDto> Comments { get; init; } = new();
}
