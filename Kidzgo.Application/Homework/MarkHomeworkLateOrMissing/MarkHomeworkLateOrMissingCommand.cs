using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Homework.MarkHomeworkLateOrMissing;

public sealed class MarkHomeworkLateOrMissingCommand : ICommand<MarkHomeworkLateOrMissingResponse>
{
    public Guid HomeworkStudentId { get; init; }
    public HomeworkStatus Status { get; init; } // Should be LATE or MISSING
}

