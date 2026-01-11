using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.CreateExamResultsBulk;

public sealed class CreateExamResultsBulkCommand : ICommand<CreateExamResultsBulkResponse>
{
    public Guid ExamId { get; init; }
    public List<ExamResultItem> Results { get; init; } = new();
}

public sealed class ExamResultItem
{
    public Guid StudentProfileId { get; init; }
    public decimal? Score { get; init; }
    public string? Comment { get; init; }
    public List<string>? AttachmentUrls { get; init; }
}

