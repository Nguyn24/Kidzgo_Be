using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.IncidentReports.Shared;

internal static class IncidentReportMapper
{
    public static IncidentReportDto ToDto(Ticket ticket)
    {
        return new IncidentReportDto
        {
            Id = ticket.Id,
            OpenedByUserId = ticket.OpenedByUserId,
            OpenedByUserName = ticket.OpenedByUser?.Name ?? string.Empty,
            BranchId = ticket.BranchId,
            BranchName = ticket.Branch?.Name ?? string.Empty,
            ClassId = ticket.ClassId,
            ClassCode = ticket.Class?.Code,
            ClassTitle = ticket.Class?.Title,
            Category = ticket.IncidentCategory?.ToString() ?? IncidentReportCategory.Other.ToString(),
            Subject = ticket.Subject,
            Message = ticket.Message,
            Status = ticket.IncidentStatus?.ToString() ?? IncidentReportStatus.Open.ToString(),
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = ticket.AssignedToUser?.Name,
            EvidenceUrl = ticket.IncidentEvidenceUrl,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            CommentCount = ticket.TicketComments.Count
        };
    }

    public static IncidentReportDetailDto ToDetailDto(Ticket ticket)
    {
        return new IncidentReportDetailDto
        {
            Id = ticket.Id,
            OpenedByUserId = ticket.OpenedByUserId,
            OpenedByUserName = ticket.OpenedByUser?.Name ?? string.Empty,
            BranchId = ticket.BranchId,
            BranchName = ticket.Branch?.Name ?? string.Empty,
            ClassId = ticket.ClassId,
            ClassCode = ticket.Class?.Code,
            ClassTitle = ticket.Class?.Title,
            Category = ticket.IncidentCategory?.ToString() ?? IncidentReportCategory.Other.ToString(),
            Subject = ticket.Subject,
            Message = ticket.Message,
            Status = ticket.IncidentStatus?.ToString() ?? IncidentReportStatus.Open.ToString(),
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = ticket.AssignedToUser?.Name,
            EvidenceUrl = ticket.IncidentEvidenceUrl,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            CommentCount = ticket.TicketComments.Count,
            Comments = ticket.TicketComments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new IncidentReportCommentDto
                {
                    Id = c.Id,
                    CommenterUserId = c.CommenterUserId,
                    CommenterUserName = c.CommenterUser?.Name ?? string.Empty,
                    Message = c.Message,
                    AttachmentUrl = c.AttachmentUrl,
                    CommentType = c.IncidentCommentType?.ToString(),
                    CreatedAt = c.CreatedAt
                })
                .ToList()
        };
    }
}
