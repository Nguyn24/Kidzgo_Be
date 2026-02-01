using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlans.DeleteLessonPlan;

public sealed class DeleteLessonPlanCommand : ICommand
{
    public Guid Id { get; init; }
}