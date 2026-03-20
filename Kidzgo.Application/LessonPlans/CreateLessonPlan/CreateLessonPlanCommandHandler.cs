using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Sessions.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.CreateLessonPlan;

public sealed class CreateLessonPlanCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateLessonPlanCommand, CreateLessonPlanResponse>
{
    public async Task<Result<CreateLessonPlanResponse>> Handle(
        CreateLessonPlanCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<CreateLessonPlanResponse>(LessonPlanErrors.Unauthorized);
        }

        // Validate class exists
        var classExists = await context.Classes
            .AnyAsync(c => c.Id == command.ClassId, cancellationToken);

        if (!classExists)
        {
            return Result.Failure<CreateLessonPlanResponse>(
                LessonPlanErrors.ClassNotFound(command.ClassId));
        }

        // Validate session exists
        var session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<CreateLessonPlanResponse>(
                SessionErrors.NotFound(command.SessionId));
        }

        // Validate session belongs to class
        if (session.ClassId != command.ClassId)
        {
            return Result.Failure<CreateLessonPlanResponse>(
                LessonPlanErrors.SessionClassMismatch(command.SessionId, command.ClassId));
        }

        // Authorization: teacher can only create lesson plans for their sessions
        if (currentUser.Role == UserRole.Teacher &&
            session.PlannedTeacherId != currentUser.Id &&
            session.ActualTeacherId != currentUser.Id)
        {
            return Result.Failure<CreateLessonPlanResponse>(LessonPlanErrors.Unauthorized);
        }

        // Check if session already has a lesson plan (not deleted)
        var existingLessonPlan = await context.LessonPlans
            .FirstOrDefaultAsync(lp => lp.SessionId == command.SessionId && !lp.IsDeleted, cancellationToken);

        if (existingLessonPlan is not null)
        {
            return Result.Failure<CreateLessonPlanResponse>(
                LessonPlanErrors.SessionAlreadyHasLessonPlan(command.SessionId));
        }

        // Validate template if provided
        if (command.TemplateId.HasValue)
        {
            var template = await context.LessonPlanTemplates
                .FirstOrDefaultAsync(t => t.Id == command.TemplateId.Value, cancellationToken);

            if (template is null)
            {
                return Result.Failure<CreateLessonPlanResponse>(
                    LessonPlanErrors.TemplateNotFound(command.TemplateId));
            }
        }

        var currentUserId = currentUser.Id;
        var now = DateTime.UtcNow;

        var lessonPlan = new LessonPlan
        {
            Id = Guid.NewGuid(),
            ClassId = command.ClassId,
            SessionId = command.SessionId,
            TemplateId = command.TemplateId,
            PlannedContent = command.PlannedContent,
            ActualContent = command.ActualContent,
            ActualHomework = command.ActualHomework,
            TeacherNotes = command.TeacherNotes,
            SubmittedBy = null,
            SubmittedAt = null,
            IsDeleted = false,
        };

        context.LessonPlans.Add(lessonPlan);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateLessonPlanResponse
        {
            Id = lessonPlan.Id,
            ClassId = lessonPlan.ClassId,
            SessionId = lessonPlan.SessionId,
            TemplateId = lessonPlan.TemplateId,
            PlannedContent = lessonPlan.PlannedContent,
            ActualContent = lessonPlan.ActualContent,
            ActualHomework = lessonPlan.ActualHomework,
            TeacherNotes = lessonPlan.TeacherNotes,
            SubmittedBy = lessonPlan.SubmittedBy,
            SubmittedAt = lessonPlan.SubmittedAt,
        };
    }
}

