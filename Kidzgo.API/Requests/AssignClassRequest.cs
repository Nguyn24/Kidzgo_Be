namespace Kidzgo.API.Requests;

public sealed class AssignClassRequest
{
    public Guid ClassId { get; set; }
    public string EntryType { get; set; } = "immediate"; // "immediate" | "makeup" | "wait"
}
