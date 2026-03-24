using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.GetPrograms;

public sealed class GetProgramsQueryHandler(
    IDbContext context
) : IQueryHandler<GetProgramsQuery, GetProgramsResponse>
{
    public async Task<Result<GetProgramsResponse>> Handle(GetProgramsQuery query, CancellationToken cancellationToken)
    {
        var programsQuery = context.Programs
            .Include(p => p.Branch)
            .Where(p => !p.IsDeleted);

        // Filter by branch
        if (query.BranchId.HasValue)
        {
            programsQuery = programsQuery.Where(p => p.BranchId == query.BranchId.Value);
        }

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            programsQuery = programsQuery.Where(p =>
                p.Name.Contains(query.SearchTerm) ||
                (p.Code != null && p.Code.Contains(query.SearchTerm)) ||
                (p.Description != null && p.Description.Contains(query.SearchTerm)));
        }

        // Filter by IsActive
        if (query.IsActive.HasValue)
        {
            programsQuery = programsQuery.Where(p => p.IsActive == query.IsActive.Value);
        }
        
        // Filter by IsMakeup
        if (query.IsMakeup.HasValue)
        {
            programsQuery = programsQuery.Where(p => p.IsMakeup == query.IsMakeup.Value);
        }

        // Get total count
        int totalCount = await programsQuery.CountAsync(cancellationToken);

        // Apply pagination
        // Sắp xếp theo CreatedAt descending để Program mới tạo nằm đầu danh sách
        var programs = await programsQuery
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Name) // Nếu có nhiều Program cùng thời điểm, sắp xếp theo tên
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(p => new ProgramDto
            {
                Id = p.Id,
                BranchId = p.BranchId,
                BranchName = p.Branch.Name,
                Name = p.Name,
                Code = p.Code,
                IsMakeup = p.IsMakeup,
                DefaultMakeupClassId = p.DefaultMakeupClassId,
                TotalSessions = p.TotalSessions,
                DefaultTuitionAmount = p.DefaultTuitionAmount,
                UnitPriceSession = p.UnitPriceSession,
                Description = p.Description,
                IsActive = p.IsActive
            })
            .ToListAsync(cancellationToken);

        var page = new Page<ProgramDto>(
            programs,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetProgramsResponse
        {
            Programs = page
        };
    }
}

