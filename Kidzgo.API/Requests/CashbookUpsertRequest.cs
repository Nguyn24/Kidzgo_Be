using Kidzgo.Domain.Finance;

namespace Kidzgo.API.Requests;

public sealed class CashbookUpsertRequest
{
    public Guid BranchId { get; set; }
    public CashbookEntryType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Title { get; set; }
    public string? Note { get; set; }
    public RelatedType? Category { get; set; }
    public DateOnly? Date { get; set; }
}
