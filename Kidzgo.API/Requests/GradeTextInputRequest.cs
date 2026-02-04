namespace Kidzgo.API.Requests;

public sealed class GradeTextInputRequest
{
    public List<TextAnswerGradeItemRequest> AnswerGrades { get; init; } = new();
}

public sealed class TextAnswerGradeItemRequest
{
    public Guid QuestionId { get; init; }
    public decimal PointsAwarded { get; init; }
    public string? TeacherFeedback { get; init; }
}


