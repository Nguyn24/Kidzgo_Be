using Kidzgo.Domain.Finance;

namespace Kidzgo.API.Requests;

public sealed class UpdateInvoiceRequest
{
    public Guid? BranchId { get; set; }
    public Guid? StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
    public InvoiceType? Type { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? Description { get; set; }
}

