namespace Kidzgo.Application.Invoices.CreatePayOSLink;

public sealed class CreatePayOSLinkResponse
{
    public Guid InvoiceId { get; init; }
    public string CheckoutUrl { get; init; } = null!;
    public string? QrCodeData { get; init; }
}

