using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.CreateMultipleChoiceHomework;
using Kidzgo.Domain.Homework;

namespace Kidzgo.Application.Homework.CreateMultipleChoiceHomeworkFromBank;

public sealed class CreateMultipleChoiceHomeworkFromBankCommand : ICommand<CreateMultipleChoiceHomeworkResponse>
{
    public Guid ClassId { get; init; }
    public Guid ProgramId { get; init; }
    public Guid? SessionId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime? DueAt { get; init; }
    public int? RewardStars { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public bool? AllowResubmit { get; init; }
    public Guid? MissionId { get; init; }
    public string? Instructions { get; init; }
    public List<QuestionLevelCountDto> Distribution { get; init; } = new();
}

public sealed class QuestionLevelCountDto
{
    public QuestionLevel Level { get; init; }
    public int Count { get; init; }
}
