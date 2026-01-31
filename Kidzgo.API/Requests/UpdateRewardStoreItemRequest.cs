namespace Kidzgo.API.Requests;

public sealed class UpdateRewardStoreItemRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? CostStars { get; set; }
    public int? Quantity { get; set; }
    public bool? IsActive { get; set; }
}

