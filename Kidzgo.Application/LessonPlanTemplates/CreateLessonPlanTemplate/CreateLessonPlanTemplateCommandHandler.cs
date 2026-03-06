using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.CreateLessonPlanTemplate;

public sealed class CreateLessonPlanTemplateCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateLessonPlanTemplateCommand, CreateLessonPlanTemplateResponse>
{
    public async Task<Result<CreateLessonPlanTemplateResponse>> Handle(
        CreateLessonPlanTemplateCommand command,
        CancellationToken cancellationToken)
    {
        // Validate program exists and is active
        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.ProgramId && !p.IsDeleted, cancellationToken);

        if (program is null)
        {
            return Result.Failure<CreateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.ProgramNotFound(command.ProgramId));
        }

        // Validate session index
        if (command.SessionIndex <= 0)
        {
            return Result.Failure<CreateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.SessionIndexRequired);
        }

        // Check for duplicate session index in the same program
        var duplicateExists = await context.LessonPlanTemplates
            .AnyAsync(t => t.ProgramId == command.ProgramId && 
                          t.SessionIndex == command.SessionIndex && 
                          !t.IsDeleted, 
                   cancellationToken);

        if (duplicateExists)
        {
            return Result.Failure<CreateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.DuplicateSessionIndex(command.ProgramId, command.SessionIndex));
        }

        var currentUserId = userContext.UserId;
        var now = DateTime.UtcNow;

        var template = new LessonPlanTemplate
        {
            Id = Guid.NewGuid(),
            ProgramId = command.ProgramId,
            Level = command.Level,
            Title = command.Title,
            SessionIndex = command.SessionIndex,
            AttachmentUrl = command.Attachment,
            IsActive = true,
            IsDeleted = false,
            CreatedBy = currentUserId,
            CreatedAt = now
        };

        context.LessonPlanTemplates.Add(template);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateLessonPlanTemplateResponse
        {
            Id = template.Id,
            ProgramId = template.ProgramId,
            Level = template.Level,
            Title = template.Title,
            SessionIndex = template.SessionIndex,
            Attachment = template.AttachmentUrl,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt
        };
    }
}

