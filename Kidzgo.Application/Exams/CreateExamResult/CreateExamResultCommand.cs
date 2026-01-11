using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.CreateExamResult;

public sealed class CreateExamResultCommand : ICommand<CreateExamResultResponse>
{
    public Guid ExamId { get; init; }
    public Guid StudentProfileId { get; init; }
    public decimal? Score { get; init; }
    public string? Comment { get; init; }
    public List<string>? AttachmentUrls { get; init; }
}

