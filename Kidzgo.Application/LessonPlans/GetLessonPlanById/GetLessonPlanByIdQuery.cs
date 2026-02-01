using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlans.GetLessonPlanById;

public sealed class GetLessonPlanByIdQuery : IQuery<GetLessonPlanByIdResponse>
{
    public Guid Id { get; init; }
}