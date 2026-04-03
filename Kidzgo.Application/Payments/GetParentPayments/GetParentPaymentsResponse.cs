using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Payments.GetParentPayments;

public sealed class GetParentPaymentsResponse
{
    public Page<ParentPaymentDto> Payments { get; init; } = null!;
}

public sealed class ParentPaymentDto
{
    public Guid Id { get; init; }
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string Method { get; init; } = null!;
    public DateTime? PaidAt { get; init; }
    public string Status { get; init; } = null!;
    public string? TransactionRef { get; init; }
}
