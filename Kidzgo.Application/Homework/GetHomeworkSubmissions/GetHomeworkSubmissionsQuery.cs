using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Homework.GetHomeworkSubmissions;

public sealed class GetHomeworkSubmissionsQuery : IQuery<GetHomeworkSubmissionsResponse>
{
    public Guid? ClassId { get; init; }
    public HomeworkStatus? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
