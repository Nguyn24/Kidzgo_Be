using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exams.UpdateExamResult;

public sealed class UpdateExamResultCommand : ICommand<UpdateExamResultResponse>
{
    public Guid Id { get; init; }
    public decimal? Score { get; init; }
    public string? Comment { get; init; }
    public List<string>? AttachmentUrls { get; init; }
}

