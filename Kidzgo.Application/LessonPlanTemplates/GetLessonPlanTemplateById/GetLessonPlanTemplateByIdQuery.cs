using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplateById;

public sealed class GetLessonPlanTemplateByIdQuery : IQuery<GetLessonPlanTemplateByIdResponse>
{
    public Guid Id { get; init; }
}