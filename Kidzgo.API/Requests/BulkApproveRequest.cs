namespace Kidzgo.API.Requests;

public sealed class BulkApproveRequest
{
    public List<Guid> Ids { get; set; } = new();
}
