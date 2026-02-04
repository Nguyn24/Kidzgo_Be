using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exercises.Student.SaveExerciseAnswer;

/// <summary>
/// UC-145: Học sinh làm Exercise (lưu câu trả lời)
/// </summary>
public sealed class SaveExerciseAnswerCommand : ICommand
{
    public Guid SubmissionId { get; init; }
    public Guid QuestionId { get; init; }
    public string Answer { get; init; } = null!;
}


