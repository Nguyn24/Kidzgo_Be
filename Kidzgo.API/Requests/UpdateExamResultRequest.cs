namespace Kidzgo.API.Requests;

public sealed class UpdateExamResultRequest
{
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
    public List<string>? AttachmentUrls { get; set; }
}

