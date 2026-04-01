using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlans.GetClassLessonPlanSyllabus;

public sealed class GetClassLessonPlanSyllabusQuery : IQuery<GetClassLessonPlanSyllabusResponse>
{
    public Guid ClassId { get; init; }
}
