using Kidzgo.Domain.Tickets;

namespace Kidzgo.API.Requests;

public sealed class CreateIncidentReportRequest
{
    public Guid BranchId { get; init; }
    public Guid? ClassId { get; init; }
    public IncidentReportCategory Category { get; init; }
    public string Subject { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? EvidenceUrl { get; init; }
}

public sealed class AssignIncidentReportRequest
{
    public Guid AssignedToUserId { get; init; }
}

public sealed class UpdateIncidentReportStatusRequest
{
    public IncidentReportStatus Status { get; init; }
}

public sealed class AddIncidentReportCommentRequest
{
    public string Message { get; init; } = null!;
    public string? AttachmentUrl { get; init; }
    public IncidentReportCommentType CommentType { get; init; } = IncidentReportCommentType.AdditionalInfo;
}
