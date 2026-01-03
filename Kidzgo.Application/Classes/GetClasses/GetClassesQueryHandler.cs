using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.GetClasses;

public sealed class GetClassesQueryHandler(
    IDbContext context
) : IQueryHandler<GetClassesQuery, GetClassesResponse>
{
    public async Task<Result<GetClassesResponse>> Handle(GetClassesQuery query, CancellationToken cancellationToken)
    {
        var classesQuery = context.Classes
            .Include(c => c.Branch)
            .Include(c => c.Program)
            .Include(c => c.MainTeacher)
            .Include(c => c.AssistantTeacher)
            .AsQueryable();

        // Filter by branch
        if (query.BranchId.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.BranchId == query.BranchId.Value);
        }

        // Filter by program
        if (query.ProgramId.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.ProgramId == query.ProgramId.Value);
        }

        // Filter by status
        if (query.Status.HasValue)
        {
            classesQuery = classesQuery.Where(c => c.Status == query.Status.Value);
        }

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            classesQuery = classesQuery.Where(c =>
                c.Code.Contains(query.SearchTerm) ||
                c.Title.Contains(query.SearchTerm));
        }

        // Get total count
        int totalCount = await classesQuery.CountAsync(cancellationToken);

        // Apply pagination
        // Sắp xếp theo CreatedAt descending để Class mới tạo nằm đầu danh sách
        var classes = await classesQuery
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.Code)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new ClassDto
            {
                Id = c.Id,
                BranchId = c.BranchId,
                BranchName = c.Branch.Name,
                ProgramId = c.ProgramId,
                ProgramName = c.Program.Name,
                Code = c.Code,
                Title = c.Title,
                MainTeacherId = c.MainTeacherId,
                MainTeacherName = c.MainTeacher != null ? c.MainTeacher.Name : null,
                AssistantTeacherId = c.AssistantTeacherId,
                AssistantTeacherName = c.AssistantTeacher != null ? c.AssistantTeacher.Name : null,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status,
                Capacity = c.Capacity,
                CurrentEnrollmentCount = c.ClassEnrollments.Count(ce => ce.Status == Domain.Classes.EnrollmentStatus.Active),
                SchedulePattern = c.SchedulePattern
            })
            .ToListAsync(cancellationToken);

        var page = new Page<ClassDto>(
            classes,
            query.PageNumber,
            query.PageSize,
            totalCount);

        return new GetClassesResponse
        {
            Classes = page
        };
    }
}

