using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.UpdateLessonPlanActual;

public sealed class UpdateLessonPlanActualCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<UpdateLessonPlanActualCommand, UpdateLessonPlanActualResponse>
{
    public async Task<Result<UpdateLessonPlanActualResponse>> Handle(
        UpdateLessonPlanActualCommand command,
        CancellationToken cancellationToken)
    {
        var lessonPlan = await context.LessonPlans
            .Include(lp => lp.Session)
            .Include(lp => lp.SubmittedByUser)
            .FirstOrDefaultAsync(lp => lp.Id == command.Id && !lp.IsDeleted, cancellationToken);

        if (lessonPlan is null)
        {
            return Result.Failure<UpdateLessonPlanActualResponse>(
                LessonPlanErrors.NotFound(command.Id));
        }

        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<UpdateLessonPlanActualResponse>(LessonPlanErrors.Unauthorized);
        }

        if (currentUser.Role == UserRole.Teacher &&
            (lessonPlan.Session is null ||
             (lessonPlan.Session.PlannedTeacherId != currentUser.Id &&
              lessonPlan.Session.ActualTeacherId != currentUser.Id)))
        {
            return Result.Failure<UpdateLessonPlanActualResponse>(LessonPlanErrors.Unauthorized);
        }

        var hasActualUpdate =
            command.ActualContent != null ||
            command.ActualHomework != null ||
            command.TeacherNotes != null;

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

        if (hasActualUpdate)
        {
            var now = DateTime.UtcNow;
            lessonPlan.SubmittedBy = currentUser.Id;
            lessonPlan.SubmittedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        var submittedByName = hasActualUpdate
            ? currentUser.Name
            : lessonPlan.SubmittedByUser?.Name;

        return new UpdateLessonPlanActualResponse
        {
            Id = lessonPlan.Id,
            SessionId = lessonPlan.SessionId,
            ActualContent = lessonPlan.ActualContent,
            ActualHomework = lessonPlan.ActualHomework,
            TeacherNotes = lessonPlan.TeacherNotes,
            SubmittedBy = lessonPlan.SubmittedBy,
            SubmittedByName = submittedByName,
            SubmittedAt = lessonPlan.SubmittedAt,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
