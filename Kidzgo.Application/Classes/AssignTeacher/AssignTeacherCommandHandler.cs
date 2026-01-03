using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.AssignTeacher;

public sealed class AssignTeacherCommandHandler(
    IDbContext context
) : ICommandHandler<AssignTeacherCommand, AssignTeacherResponse>
{
    public async Task<Result<AssignTeacherResponse>> Handle(AssignTeacherCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .Include(c => c.MainTeacher)
            .Include(c => c.AssistantTeacher)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<AssignTeacherResponse>(
                Error.NotFound("Class.NotFound", "Class not found"));
        }

        // Check if main teacher exists, is TEACHER role, and belongs to the same branch
        if (command.MainTeacherId.HasValue)
        {
            var mainTeacher = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.MainTeacherId.Value && u.Role == Domain.Users.UserRole.Teacher, cancellationToken);

            if (mainTeacher is null)
            {
                return Result.Failure<AssignTeacherResponse>(
                    Error.NotFound("Class.MainTeacherNotFound", "Main teacher not found or is not a teacher"));
            }

            // Check if teacher belongs to the same branch as the class
            if (mainTeacher.BranchId != classEntity.BranchId)
            {
                return Result.Failure<AssignTeacherResponse>(
                    Error.Conflict("Class.MainTeacherBranchMismatch", "Main teacher must belong to the same branch as the class"));
            }

            classEntity.MainTeacherId = command.MainTeacherId.Value;
        }
        else
        {
            classEntity.MainTeacherId = null;
        }

        // Check if assistant teacher exists, is TEACHER role, and belongs to the same branch
        if (command.AssistantTeacherId.HasValue)
        {
            var assistantTeacher = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.AssistantTeacherId.Value && u.Role == Domain.Users.UserRole.Teacher, cancellationToken);

            if (assistantTeacher is null)
            {
                return Result.Failure<AssignTeacherResponse>(
                    Error.NotFound("Class.AssistantTeacherNotFound", "Assistant teacher not found or is not a teacher"));
            }

            // Check if teacher belongs to the same branch as the class
            if (assistantTeacher.BranchId != classEntity.BranchId)
            {
                return Result.Failure<AssignTeacherResponse>(
                    Error.Conflict("Class.AssistantTeacherBranchMismatch", "Assistant teacher must belong to the same branch as the class"));
            }

            classEntity.AssistantTeacherId = command.AssistantTeacherId.Value;
        }
        else
        {
            classEntity.AssistantTeacherId = null;
        }

        classEntity.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        // Re-query to get teacher names
        var updatedClass = await context.Classes
            .Include(c => c.MainTeacher)
            .Include(c => c.AssistantTeacher)
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        return new AssignTeacherResponse
        {
            ClassId = updatedClass!.Id,
            MainTeacherId = updatedClass.MainTeacherId,
            MainTeacherName = updatedClass.MainTeacher?.Name,
            AssistantTeacherId = updatedClass.AssistantTeacherId,
            AssistantTeacherName = updatedClass.AssistantTeacher?.Name
        };
    }
}

