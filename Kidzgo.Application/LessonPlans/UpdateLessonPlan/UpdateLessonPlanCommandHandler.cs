using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.UpdateLessonPlan;

public sealed class UpdateLessonPlanCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<UpdateLessonPlanCommand, UpdateLessonPlanResponse>
{
    public async Task<Result<UpdateLessonPlanResponse>> Handle(
        UpdateLessonPlanCommand command,
        CancellationToken cancellationToken)
    {
        var lessonPlan = await context.LessonPlans
            .Include(lp => lp.Session)
            .FirstOrDefaultAsync(lp => lp.Id == command.Id && !lp.IsDeleted, cancellationToken);

        if (lessonPlan is null)
        {
            return Result.Failure<UpdateLessonPlanResponse>(
                LessonPlanErrors.NotFound(command.Id));
        }

        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<UpdateLessonPlanResponse>(LessonPlanErrors.Unauthorized);
        }

        if (currentUser.Role == UserRole.Teacher &&
            (lessonPlan.Session is null ||
             (lessonPlan.Session.PlannedTeacherId != currentUser.Id &&
              lessonPlan.Session.ActualTeacherId != currentUser.Id)))
        {
            return Result.Failure<UpdateLessonPlanResponse>(LessonPlanErrors.Unauthorized);
        }

        // Validate template if provided
        if (command.TemplateId.HasValue)
        {
            var template = await context.LessonPlanTemplates
                .FirstOrDefaultAsync(t => t.Id == command.TemplateId.Value, cancellationToken);

            if (template is null)
            {
                return Result.Failure<UpdateLessonPlanResponse>(
                    LessonPlanErrors.TemplateNotFound(command.TemplateId));
            }
        }

        // Update fields
        if (command.TemplateId.HasValue)
        {
            lessonPlan.TemplateId = command.TemplateId.Value;
        }

        if (command.PlannedContent != null)
        {
            lessonPlan.PlannedContent = command.PlannedContent;
        }

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

        return new UpdateLessonPlanResponse
        {
            Id = lessonPlan.Id,
            SessionId = lessonPlan.SessionId,
            TemplateId = lessonPlan.TemplateId,
            PlannedContent = lessonPlan.PlannedContent,
            ActualContent = lessonPlan.ActualContent,
            ActualHomework = lessonPlan.ActualHomework,
            TeacherNotes = lessonPlan.TeacherNotes
        };
    }
}

