namespace Kidzgo.API.Requests;

public sealed class CreateBranchRequest
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}

