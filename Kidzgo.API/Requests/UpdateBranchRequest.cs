namespace Kidzgo.API.Requests;

public sealed class UpdateBranchRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}

