namespace Kidzgo.Application.Abstraction.Payments;

public interface IPayOSService
{
    Task<PayOSCreateLinkResponse> CreatePaymentLinkAsync(
        PayOSCreateLinkRequest request,
        CancellationToken cancellationToken = default);

    bool VerifyWebhookSignature(string signature, string data);
}

public sealed class PayOSCreateLinkRequest
{
    public long OrderCode { get; init; }
    public int Amount { get; init; } // Amount in VND (đồng)
    public string Description { get; init; } = null!;
    public List<PayOSItem>? Items { get; init; }
    public string CancelUrl { get; init; } = null!;
    public string ReturnUrl { get; init; } = null!;
    public int? ExpiredAt { get; init; } // Unix timestamp (seconds)
}

public sealed class PayOSItem
{
    public string Name { get; init; } = null!;
    public int Quantity { get; init; }
    public int Price { get; init; } // Price in VND (đồng)
}

public sealed class PayOSCreateLinkResponse
{
    public int Error { get; init; }
    public string? Message { get; init; }
    public PayOSData? Data { get; init; }
}

public sealed class PayOSData
{
    public string CheckoutUrl { get; init; } = null!;
    public string QrCode { get; init; } = null!;
}

public sealed class PayOSWebhookRequest
{
    public int Code { get; init; }
    public string Desc { get; init; } = null!;
    public PayOSWebhookData? Data { get; init; }
    public string? Signature { get; init; }
}

public sealed class PayOSWebhookData
{
    public long OrderCode { get; init; }
    public int Amount { get; init; }
    public string Description { get; init; } = null!;
    public string AccountNumber { get; init; } = null!;
    public string Reference { get; init; } = null!;
    public string TransactionDateTime { get; init; } = null!;
    public string Currency { get; init; } = null!;
    public string PaymentLinkId { get; init; } = null!;
    public int Code { get; init; }
    public string Desc { get; init; } = null!;
    public bool CounterAccountBankId { get; init; }
    public string CounterAccountBankName { get; init; } = null!;
    public string CounterAccountName { get; init; } = null!;
    public string CounterAccountNumber { get; init; } = null!;
    public string VirtualAccountName { get; init; } = null!;
    public string VirtualAccountNumber { get; init; } = null!;
}

