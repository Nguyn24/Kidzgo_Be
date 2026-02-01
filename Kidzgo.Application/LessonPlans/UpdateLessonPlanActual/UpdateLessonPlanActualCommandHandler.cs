using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.UpdateLessonPlanActual;

public sealed class UpdateLessonPlanActualCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateLessonPlanActualCommand, UpdateLessonPlanActualResponse>
{
    public async Task<Result<UpdateLessonPlanActualResponse>> Handle(
        UpdateLessonPlanActualCommand command,
        CancellationToken cancellationToken)
    {
        var lessonPlan = await context.LessonPlans
            .FirstOrDefaultAsync(lp => lp.Id == command.Id && !lp.IsDeleted, cancellationToken);

        if (lessonPlan is null)
        {
            return Result.Failure<UpdateLessonPlanActualResponse>(
                LessonPlanErrors.NotFound(command.Id));
        }

        // Update only actual content fields (PATCH - partial update)
        if (command.ActualContent != null)
        {
            lessonPlan.ActualContent = command.ActualContent;
        }

        if (command.ActualHomework != null)
        {
            lessonPlan.ActualHomework = command.ActualHomework;
        }

        if (command.TeacherNotes != null)
        {
            lessonPlan.TeacherNotes = command.TeacherNotes;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateLessonPlanActualResponse
        {
            Id = lessonPlan.Id,
            SessionId = lessonPlan.SessionId,
            ActualContent = lessonPlan.ActualContent,
            ActualHomework = lessonPlan.ActualHomework,
            TeacherNotes = lessonPlan.TeacherNotes,
            UpdatedAt = DateTime.UtcNow
        };
    }
}