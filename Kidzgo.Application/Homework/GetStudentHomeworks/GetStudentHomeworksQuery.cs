using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Homework.GetStudentHomeworks;

public sealed class GetStudentHomeworksQuery : IQuery<GetStudentHomeworksResponse>, IPageableQuery
{
    public HomeworkStatus? Status { get; init; }
    public Guid? ClassId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

