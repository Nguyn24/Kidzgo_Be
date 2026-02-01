using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Homework.GradeHomework;

public sealed class GradeHomeworkCommand : ICommand<GradeHomeworkResponse>
{
    public Guid HomeworkStudentId { get; init; }
    public decimal Score { get; init; }
    public string? TeacherFeedback { get; init; }
}

