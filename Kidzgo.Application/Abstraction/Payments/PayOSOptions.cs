namespace Kidzgo.Application.Abstraction.Payments;

public sealed class PayOSOptions
{
    public const string SectionName = "PayOS";

    public string BaseUrl { get; set; } = "https://api.payos.vn";
    public int ClientId { get; set; }
    public string ApiKey { get; set; } = null!;
    public string ChecksumKey { get; set; } = null!;
    public string ReturnUrl { get; set; } = null!;
    public string CancelUrl { get; set; } = null!;
}


