using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

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
        var classrooms = await classroomsQuery
            .OrderBy(c => c.Name)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(c => new ClassroomDto
            {
                Id = c.Id,
                BranchId = c.BranchId,
                BranchName = c.Branch.Name,
                Name = c.Name,
                Capacity = c.Capacity,
                Note = c.Note,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);

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

