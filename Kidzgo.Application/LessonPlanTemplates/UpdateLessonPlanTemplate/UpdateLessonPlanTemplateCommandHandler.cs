using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.UpdateLessonPlanTemplate;

public sealed class UpdateLessonPlanTemplateCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateLessonPlanTemplateCommand, UpdateLessonPlanTemplateResponse>
{
    public async Task<Result<UpdateLessonPlanTemplateResponse>> Handle(
        UpdateLessonPlanTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var template = await context.LessonPlanTemplates
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken);

        if (template is null)
        {
            return Result.Failure<UpdateLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.NotFound(command.Id));
        }

        // Validate session index if provided
        if (command.SessionIndex.HasValue)
        {
            if (command.SessionIndex.Value <= 0)
            {
                return Result.Failure<UpdateLessonPlanTemplateResponse>(
                    LessonPlanTemplateErrors.SessionIndexRequired);
            }

            // Check for duplicate session index in the same program
            var duplicateExists = await context.LessonPlanTemplates
                .AnyAsync(t => t.ProgramId == template.ProgramId && 
                              t.SessionIndex == command.SessionIndex.Value && 
                              t.Id != command.Id && 
                              !t.IsDeleted, 
                       cancellationToken);

            if (duplicateExists)
            {
                return Result.Failure<UpdateLessonPlanTemplateResponse>(
                    LessonPlanTemplateErrors.DuplicateSessionIndex(template.ProgramId, command.SessionIndex.Value));
            }
        }

        // Update fields
        if (command.Level != null)
        {
            template.Level = command.Level;
        }

        if (command.SessionIndex.HasValue)
        {
            template.SessionIndex = command.SessionIndex.Value;
        }

        if (command.StructureJson != null)
        {
            template.StructureJson = command.StructureJson;
        }

        if (command.IsActive.HasValue)
        {
            template.IsActive = command.IsActive.Value;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateLessonPlanTemplateResponse
        {
            Id = template.Id,
            ProgramId = template.ProgramId,
            Level = template.Level,
            SessionIndex = template.SessionIndex,
            StructureJson = template.StructureJson,
            IsActive = template.IsActive
        };
    }
}

