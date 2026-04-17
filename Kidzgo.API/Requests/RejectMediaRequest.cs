namespace Kidzgo.API.Requests;

public sealed record RejectMediaRequest
{
    public string Reason { get; init; } = null!;
}
