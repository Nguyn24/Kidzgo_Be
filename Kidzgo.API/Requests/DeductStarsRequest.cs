namespace Kidzgo.API.Requests;

public sealed class DeductStarsRequest
{
    public Guid StudentProfileId { get; set; }
    public int Amount { get; set; }
    public string? Reason { get; set; }
}

