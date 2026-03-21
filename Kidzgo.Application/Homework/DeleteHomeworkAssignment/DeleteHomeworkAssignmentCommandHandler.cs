using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.DeleteHomeworkAssignment;

public sealed class DeleteHomeworkAssignmentCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteHomeworkAssignmentCommand>
{
    public async Task<Result> Handle(
        DeleteHomeworkAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        var homework = await context.HomeworkAssignments
            .FirstOrDefaultAsync(h => h.Id == command.Id, cancellationToken);

        if (homework is null)
        {
            return Result.Failure(HomeworkErrors.NotFound(command.Id));
        }

        // Hard delete: Remove homework assignment
        // Note: Based on HomeworkAssignmentConfiguration, HomeworkStudents have Cascade delete
        // This means all HomeworkStudent records will also be deleted
        // If you need to keep history, change DeleteBehavior to Restrict in configuration
        context.HomeworkAssignments.Remove(homework);
        
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

