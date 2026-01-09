using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.CreateClass;

public sealed class CreateClassCommandHandler(
    IDbContext context
) : ICommandHandler<CreateClassCommand, CreateClassResponse>
{
    public async Task<Result<CreateClassResponse>> Handle(CreateClassCommand command, CancellationToken cancellationToken)
    {
        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<CreateClassResponse>(
                ClassErrors.BranchNotFound);
        }

        // Check if program exists
        bool programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<CreateClassResponse>(
                ClassErrors.ProgramNotFound);
        }

        // Check if code is unique
        bool codeExists = await context.Classes
            .AnyAsync(c => c.Code == command.Code, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<CreateClassResponse>(
                ClassErrors.CodeExists);
        }

        // Check if teachers exist, are TEACHER role, and belong to the same branch
        if (command.MainTeacherId.HasValue)
        {
            var mainTeacher = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.MainTeacherId.Value && u.Role == Domain.Users.UserRole.Teacher, cancellationToken);

            if (mainTeacher is null)
            {
                return Result.Failure<CreateClassResponse>(
                    ClassErrors.MainTeacherNotFound);
            }

            // Check if teacher belongs to the same branch as the class
            if (mainTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<CreateClassResponse>(
                    ClassErrors.MainTeacherBranchMismatch);
            }
        }

        if (command.AssistantTeacherId.HasValue)
        {
            var assistantTeacher = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.AssistantTeacherId.Value && u.Role == Domain.Users.UserRole.Teacher, cancellationToken);

            if (assistantTeacher is null)
            {
                return Result.Failure<CreateClassResponse>(
                    ClassErrors.AssistantTeacherNotFound);
            }

            // Check if teacher belongs to the same branch as the class
            if (assistantTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<CreateClassResponse>(
                    ClassErrors.AssistantTeacherBranchMismatch);
            }
        }

        var now = DateTime.UtcNow;
        var classEntity = new Class
        {
            Id = Guid.NewGuid(),
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
            Code = command.Code,
            Title = command.Title,
            MainTeacherId = command.MainTeacherId,
            AssistantTeacherId = command.AssistantTeacherId,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Status = ClassStatus.Planned, // Mặc định PLANNED
            Capacity = command.Capacity,
            SchedulePattern = command.SchedulePattern,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Classes.Add(classEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateClassResponse
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

