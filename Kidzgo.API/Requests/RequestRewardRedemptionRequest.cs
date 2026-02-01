namespace Kidzgo.API.Requests;

public sealed class RequestRewardRedemptionRequest
{
    public Guid ItemId { get; set; }
    public int Quantity { get; set; } = 1;
}

