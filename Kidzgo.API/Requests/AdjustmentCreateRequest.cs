namespace Kidzgo.API.Requests;

public sealed class AdjustmentCreateRequest
{
    public Guid BranchId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Note { get; set; }
    public DateTime? Timestamp { get; set; }
}
