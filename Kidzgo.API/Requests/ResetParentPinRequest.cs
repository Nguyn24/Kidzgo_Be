namespace Kidzgo.API.Requests;

public sealed class ResetParentPinRequest
{
    public string Token { get; set; } = null!;
    public string NewPin { get; set; } = null!;
}
