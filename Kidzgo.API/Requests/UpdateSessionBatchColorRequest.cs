namespace Kidzgo.API.Requests;

public sealed class UpdateSessionBatchColorRequest
{
    public Guid ClassId { get; set; }
    public string Color { get; set; } = string.Empty;
}
