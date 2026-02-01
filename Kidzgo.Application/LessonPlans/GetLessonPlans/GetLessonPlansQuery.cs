using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlans.GetLessonPlans;

public sealed class GetLessonPlansQuery : IQuery<GetLessonPlansResponse>
{
    public Guid? SessionId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? TemplateId { get; init; }
    public Guid? SubmittedBy { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public bool IncludeDeleted { get; init; } = false;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}