using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Homework.GetStudentHomeworkFeedback;

public sealed class GetStudentHomeworkFeedbackQuery : IQuery<GetStudentHomeworkFeedbackResponse>, IPageableQuery
{
    public Guid? ClassId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

