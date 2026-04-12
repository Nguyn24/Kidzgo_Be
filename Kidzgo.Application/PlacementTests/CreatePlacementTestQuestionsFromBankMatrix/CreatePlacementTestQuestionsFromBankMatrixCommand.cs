using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Homework;

namespace Kidzgo.Application.PlacementTests.CreatePlacementTestQuestionsFromBankMatrix;

public sealed class CreatePlacementTestQuestionsFromBankMatrixCommand : ICommand<CreatePlacementTestQuestionsFromBankMatrixResponse>
{
    public Guid PlacementTestId { get; init; }
    public Guid? ProgramId { get; init; }
    public HomeworkQuestionType QuestionType { get; init; }
    public string? Skill { get; init; }
    public string? Topic { get; init; }
    public bool ShuffleQuestions { get; init; } = true;
    public int TotalQuestions { get; init; }
    public List<PlacementTestQuestionMatrixLevelCountDto> Distribution { get; init; } = new();
}

public sealed class PlacementTestQuestionMatrixLevelCountDto
{
    public QuestionLevel Level { get; init; }
    public int Count { get; init; }
}
