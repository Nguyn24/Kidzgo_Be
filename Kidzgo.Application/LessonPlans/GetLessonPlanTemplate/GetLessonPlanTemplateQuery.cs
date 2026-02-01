using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlans.GetLessonPlanTemplate;

public sealed class GetLessonPlanTemplateQuery : IQuery<GetLessonPlanTemplateResponse>
{
    public Guid LessonPlanId { get; init; }
}