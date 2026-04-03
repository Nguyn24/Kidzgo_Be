using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Classrooms.GetClassroomById;

public sealed class GetClassroomByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetClassroomByIdQuery, GetClassroomByIdResponse>
{
    public async Task<Result<GetClassroomByIdResponse>> Handle(GetClassroomByIdQuery query, CancellationToken cancellationToken)
    {
        var classroomRow = await context.Classrooms
            .Include(c => c.Branch)
            .Where(c => c.Id == query.Id)
            .Select(c => new
            {
                c.Id,
                c.BranchId,
                BranchName = c.Branch.Name,
                c.Name,
                c.Capacity,
                c.Note,
                c.IsActive,
                c.Floor,
                c.Area,
                c.EquipmentJson,
                UtilizationPercent = c.Capacity <= 0
                    ? 0
                    : Math.Round(
                        (decimal)context.Sessions.Count(s =>
                            (s.PlannedRoomId == c.Id || s.ActualRoomId == c.Id) &&
                            s.PlannedDatetime >= DateTime.UtcNow.Date &&
                            s.PlannedDatetime < DateTime.UtcNow.Date.AddDays(30)) * 100 / c.Capacity,
                        2)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (classroomRow is null)
        {
            return Result.Failure<GetClassroomByIdResponse>(ClassroomErrors.NotFound(query.Id));
        }

        return new GetClassroomByIdResponse
        {
            Id = classroomRow.Id,
            BranchId = classroomRow.BranchId,
            BranchName = classroomRow.BranchName,
            Name = classroomRow.Name,
            Capacity = classroomRow.Capacity,
            Note = classroomRow.Note,
            IsActive = classroomRow.IsActive,
            Floor = classroomRow.Floor,
            Area = classroomRow.Area,
            Equipment = string.IsNullOrWhiteSpace(classroomRow.EquipmentJson)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(classroomRow.EquipmentJson!) ?? new List<string>(),
            UtilizationPercent = classroomRow.UtilizationPercent
        };
    }
}

