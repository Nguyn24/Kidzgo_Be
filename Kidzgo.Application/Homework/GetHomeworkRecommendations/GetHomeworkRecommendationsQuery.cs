using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.GetHomeworkRecommendations;

public sealed class GetHomeworkRecommendationsQuery : IQuery<GetHomeworkRecommendationsResponse>
{
    public Guid HomeworkStudentId { get; init; }
    public string? CurrentAnswerText { get; init; }
    public string Language { get; init; } = "vi";
    public int MaxItems { get; init; } = 5;
}
