namespace Kidzgo.API.Requests;

public sealed class CreateMakeupCreditRequest
{
    public Guid StudentProfileId { get; set; }
    public Guid SourceSessionId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string CreatedReason { get; set; } = "LongTerm";
}

public sealed class UseMakeupCreditRequest
{
    public Guid TargetSessionId { get; set; }
    public Guid? AssignedBy { get; set; }
}

public sealed class ExpireMakeupCreditRequest
{
    public DateTime? ExpiresAt { get; set; }
}

