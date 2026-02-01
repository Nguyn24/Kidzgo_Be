using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans.Errors;

public static class LessonPlanErrors
{
    public static Error NotFound(Guid? lessonPlanId) => Error.NotFound(
        "LessonPlan.NotFound",
        $"Lesson plan with Id = '{lessonPlanId}' was not found");

    public static Error SessionNotFound(Guid? sessionId) => Error.NotFound(
        "LessonPlan.SessionNotFound",
        $"Session with Id = '{sessionId}' was not found");

    public static Error TemplateNotFound(Guid? templateId) => Error.NotFound(
        "LessonPlan.TemplateNotFound",
        $"Lesson plan template with Id = '{templateId}' was not found");

    public static readonly Error SessionRequired = Error.Validation(
        "LessonPlan.SessionRequired",
        "SessionId is required");

    public static Error SessionAlreadyHasLessonPlan(Guid? sessionId) => Error.Conflict(
        "LessonPlan.SessionAlreadyHasLessonPlan",
        $"Session with Id = '{sessionId}' already has a lesson plan");

    public static readonly Error Unauthorized = Error.Validation(
        "LessonPlan.Unauthorized",
        "You do not have permission to access this lesson plan");
}