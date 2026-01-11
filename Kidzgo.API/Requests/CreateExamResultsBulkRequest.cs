namespace Kidzgo.API.Requests;

public sealed class CreateExamResultsBulkRequest
{
    public List<ExamResultItemRequest> Results { get; set; } = new();
}

public sealed class ExamResultItemRequest
{
    public Guid StudentProfileId { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
    public List<string>? AttachmentUrls { get; set; }
}

