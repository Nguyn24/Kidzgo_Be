using Kidzgo.Domain.Tickets;

namespace Kidzgo.Application.Tickets.GetTicketById;

public sealed class GetTicketByIdResponse
{
    public Guid Id { get; init; }
    public Guid OpenedByUserId { get; init; }
    public string OpenedByUserName { get; init; } = null!;
    public Guid? OpenedByProfileId { get; init; }
    public string? OpenedByProfileName { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public Guid? ClassId { get; init; }
    public string? ClassCode { get; init; }
    public string? ClassTitle { get; init; }
    public TicketCategory Category { get; init; }
    public string Message { get; init; } = null!;
    public TicketStatus Status { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string? AssignedToUserName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<TicketCommentDto> Comments { get; init; } = new();
}

public sealed class TicketCommentDto
{
    public Guid Id { get; init; }
    public Guid CommenterUserId { get; init; }
    public string CommenterUserName { get; init; } = null!;
    public Guid? CommenterProfileId { get; init; }
    public string? CommenterProfileName { get; init; }
    public string Message { get; init; } = null!;
    public string? AttachmentUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}

