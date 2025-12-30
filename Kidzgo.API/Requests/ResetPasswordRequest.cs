namespace Kidzgo.API.Requests;

public sealed class ResetPasswordRequest
{
    public string Token { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}


