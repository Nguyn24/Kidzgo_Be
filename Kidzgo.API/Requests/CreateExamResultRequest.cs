namespace Kidzgo.API.Requests;

public sealed class CreateExamResultRequest
{
    public Guid StudentProfileId { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
    public List<string>? AttachmentUrls { get; set; }
}

