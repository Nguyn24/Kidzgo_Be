namespace Kidzgo.API.Requests;

public sealed class VerifyParentPinRequest
{
    public Guid ProfileId { get; set; }
    public string Pin { get; set; } = null!;
}
























