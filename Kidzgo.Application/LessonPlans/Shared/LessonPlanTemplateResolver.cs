using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.Shared;

internal static class LessonPlanTemplateResolver
{
    public static async Task<LessonPlanTemplate?> ResolveForSessionAsync(
        IDbContext context,
        Guid classId,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var classProgram = await context.Classes
            .Where(c => c.Id == classId)
            .Select(c => new
            {
                c.Id,
                c.ProgramId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (classProgram is null)
        {
            return null;
        }

        var orderedSessionIds = await context.Sessions
            .Where(s => s.ClassId == classId)
            .OrderBy(s => s.PlannedDatetime)
            .ThenBy(s => s.CreatedAt)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        var sessionIndex = orderedSessionIds.FindIndex(id => id == sessionId);
        if (sessionIndex < 0)
        {
            return null;
        }

        return await context.LessonPlanTemplates
            .FirstOrDefaultAsync(
                t => t.ProgramId == classProgram.ProgramId &&
                     t.SessionIndex == sessionIndex + 1 &&
                     t.IsActive &&
                     !t.IsDeleted,
                cancellationToken);
    }
}
