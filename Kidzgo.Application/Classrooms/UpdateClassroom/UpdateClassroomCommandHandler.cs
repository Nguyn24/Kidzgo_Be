using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Classrooms.UpdateClassroom;

public sealed class UpdateClassroomCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateClassroomCommand, UpdateClassroomResponse>
{
    public async Task<Result<UpdateClassroomResponse>> Handle(UpdateClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroom = await context.Classrooms
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classroom is null)
        {
            return Result.Failure<UpdateClassroomResponse>(ClassroomErrors.NotFound(command.Id));
        }

        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<UpdateClassroomResponse>(ClassroomErrors.BranchNotFound);
        }

        var equipment = command.Equipment ?? new List<string>();

        classroom.BranchId = command.BranchId;
        classroom.Name = command.Name;
        classroom.Capacity = command.Capacity;
        classroom.Note = command.Note;
        classroom.Floor = command.Floor;
        classroom.Area = command.Area;
        classroom.EquipmentJson = equipment.Count > 0 ? JsonSerializer.Serialize(equipment) : null;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateClassroomResponse
        {
            Id = classroom.Id,
            BranchId = classroom.BranchId,
            Name = classroom.Name,
            Capacity = classroom.Capacity,
            Note = classroom.Note,
            Floor = classroom.Floor,
            Area = classroom.Area,
            Equipment = equipment,
            IsActive = classroom.IsActive
        };
    }
}
