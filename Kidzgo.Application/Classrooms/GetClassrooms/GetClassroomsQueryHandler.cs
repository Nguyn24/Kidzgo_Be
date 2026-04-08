using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Classrooms.GetClassrooms;

public sealed class GetClassroomsQueryHandler(
    IDbContext context
) : IQueryHandler<GetClassroomsQuery, GetClassroomsResponse>
{
    public async Task<Result<GetClassroomsResponse>> Handle(GetClassroomsQuery query, CancellationToken cancellationToken)
    {
        var classroomsQuery = context.Classrooms
            .Include(c => c.Branch)
            .AsQueryable();

        // Filter by branch
        if (query.BranchId.HasValue)
        {
            classroomsQuery = classroomsQuery.Where(c => c.BranchId == query.BranchId.Value);
        }

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            classroomsQuery = classroomsQuery.Where(c =>
                c.Name.Contains(query.SearchTerm) ||
                (c.Note != null && c.Note.Contains(query.SearchTerm)));
        }

        // Filter by IsActive
        if (query.IsActive.HasValue)
        {
            classroomsQuery = classroomsQuery.Where(c => c.IsActive == query.IsActive.Value);
        }

        // Get total count
        int totalCount = await classroomsQuery.CountAsync(cancellationToken);

        // Apply pagination
        // Sắp xếp theo Name để dễ tìm
        var classroomRows = await classroomsQuery
            .OrderBy(c => c.Name)
            .ApplyPagination(query.PageNumber, query.PageSize)
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
                            s.PlannedDatetime >= VietnamTime.TodayStartUtc() &&
                            s.PlannedDatetime < VietnamTime.TodayStartUtc().AddDays(30)) * 100 / c.Capacity,
                        2)
            })
            .ToListAsync(cancellationToken);

        var classrooms = classroomRows
            .Select(c => new ClassroomDto
            {
                Id = c.Id,
                BranchId = c.BranchId,
                BranchName = c.BranchName,
                Name = c.Name,
                Capacity = c.Capacity,
                Note = c.Note,
                IsActive = c.IsActive,
                Floor = c.Floor,
                Area = c.Area,
                Equipment = string.IsNullOrWhiteSpace(c.EquipmentJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(c.EquipmentJson!) ?? new List<string>(),
                UtilizationPercent = c.UtilizationPercent
            })
            .ToList();

        var page = new Page<ClassroomDto>(
            classrooms,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetClassroomsResponse
        {
            Classrooms = page
        };
    }
}

