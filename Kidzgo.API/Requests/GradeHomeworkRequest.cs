namespace Kidzgo.API.Requests;

public sealed class GradeHomeworkRequest
{
    public Guid HomeworkStudentId { get; init; }
    public decimal Score { get; init; }
    public string? TeacherFeedback { get; init; }
}

