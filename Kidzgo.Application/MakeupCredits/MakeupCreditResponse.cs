using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.MakeupCredits;

public sealed class MakeupCreditResponse
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid SourceSessionId { get; set; }
    public MakeupCreditStatus Status { get; set; }
    public CreatedReason CreatedReason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? UsedSessionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

