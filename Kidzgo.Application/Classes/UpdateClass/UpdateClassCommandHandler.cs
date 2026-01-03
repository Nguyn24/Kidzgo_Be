using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.UpdateClass;

public sealed class UpdateClassCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateClassCommand, UpdateClassResponse>
{
    public async Task<Result<UpdateClassResponse>> Handle(UpdateClassCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<UpdateClassResponse>(
                Error.NotFound("Class.NotFound", "Class not found"));
        }

        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<UpdateClassResponse>(
                Error.NotFound("Class.BranchNotFound", "Branch not found or inactive"));
        }

        // Check if program exists
        bool programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<UpdateClassResponse>(
                Error.NotFound("Class.ProgramNotFound", "Program not found, deleted, or inactive"));
        }

        // Check if code is unique (excluding current class)
        bool codeExists = await context.Classes
            .AnyAsync(c => c.Code == command.Code && c.Id != command.Id, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<UpdateClassResponse>(
                Error.Conflict("Class.CodeExists", "Class code already exists"));
        }

        // Check if teachers exist, are TEACHER role, and belong to the same branch
        if (command.MainTeacherId.HasValue)
        {
            var mainTeacher = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.MainTeacherId.Value && u.Role == Domain.Users.UserRole.Teacher, cancellationToken);

            if (mainTeacher is null)
            {
                return Result.Failure<UpdateClassResponse>(
                    Error.NotFound("Class.MainTeacherNotFound", "Main teacher not found or is not a teacher"));
            }

            // Check if teacher belongs to the same branch as the class
            if (mainTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<UpdateClassResponse>(
                    Error.Conflict("Class.MainTeacherBranchMismatch", "Main teacher must belong to the same branch as the class"));
            }
        }

        if (command.AssistantTeacherId.HasValue)
        {
            var assistantTeacher = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.AssistantTeacherId.Value && u.Role == Domain.Users.UserRole.Teacher, cancellationToken);

            if (assistantTeacher is null)
            {
                return Result.Failure<UpdateClassResponse>(
                    Error.NotFound("Class.AssistantTeacherNotFound", "Assistant teacher not found or is not a teacher"));
            }

            // Check if teacher belongs to the same branch as the class
            if (assistantTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<UpdateClassResponse>(
                    Error.Conflict("Class.AssistantTeacherBranchMismatch", "Assistant teacher must belong to the same branch as the class"));
            }
        }

        classEntity.BranchId = command.BranchId;
        classEntity.ProgramId = command.ProgramId;
        classEntity.Code = command.Code;
        classEntity.Title = command.Title;
        classEntity.MainTeacherId = command.MainTeacherId;
        classEntity.AssistantTeacherId = command.AssistantTeacherId;
        classEntity.StartDate = command.StartDate;
        classEntity.EndDate = command.EndDate;
        classEntity.Capacity = command.Capacity;
        classEntity.SchedulePattern = command.SchedulePattern;
        classEntity.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateClassResponse
        {
            Id = classEntity.Id,
            BranchId = classEntity.BranchId,
            ProgramId = classEntity.ProgramId,
            Code = classEntity.Code,
            Title = classEntity.Title,
            MainTeacherId = classEntity.MainTeacherId,
            AssistantTeacherId = classEntity.AssistantTeacherId,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            Status = classEntity.Status,
            Capacity = classEntity.Capacity,
            SchedulePattern = classEntity.SchedulePattern
        };
    }
}

