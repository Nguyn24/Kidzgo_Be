using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Exercises.Student.StartExerciseSubmission;

/// <summary>
/// UC-145: Học sinh làm Exercise (bắt đầu làm -> tạo submission nếu chưa có)
/// </summary>
public sealed class StartExerciseSubmissionCommand : ICommand<StartExerciseSubmissionResponse>
{
    public Guid ExerciseId { get; init; }
}


