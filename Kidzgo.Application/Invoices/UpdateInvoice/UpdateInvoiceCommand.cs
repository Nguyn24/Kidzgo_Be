using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Finance;

namespace Kidzgo.Application.Invoices.UpdateInvoice;

public sealed class UpdateInvoiceCommand : ICommand<UpdateInvoiceResponse>
{
    public Guid Id { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public Guid? ClassId { get; init; }
    public InvoiceType? Type { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public DateOnly? DueDate { get; init; }
    public string? Description { get; init; }
}

