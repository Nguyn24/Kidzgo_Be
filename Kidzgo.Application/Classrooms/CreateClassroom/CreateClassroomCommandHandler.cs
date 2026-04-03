using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Classrooms.CreateClassroom;

public sealed class CreateClassroomCommandHandler(
    IDbContext context
) : ICommandHandler<CreateClassroomCommand, CreateClassroomResponse>
{
    public async Task<Result<CreateClassroomResponse>> Handle(CreateClassroomCommand command, CancellationToken cancellationToken)
    {
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<CreateClassroomResponse>(ClassroomErrors.BranchNotFound);
        }

        var equipment = command.Equipment ?? new List<string>();

        var classroom = new Classroom
        {
            Id = Guid.NewGuid(),
            BranchId = command.BranchId,
            Name = command.Name,
            Capacity = command.Capacity,
            Note = command.Note,
            Floor = command.Floor,
            Area = command.Area,
            EquipmentJson = equipment.Count > 0 ? JsonSerializer.Serialize(equipment) : null,
            IsActive = true
        };

        context.Classrooms.Add(classroom);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateClassroomResponse
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
