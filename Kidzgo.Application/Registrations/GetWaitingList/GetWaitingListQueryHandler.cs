using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetWaitingList.Handler;

public sealed class GetWaitingListQueryHandler(
    IDbContext context
) : IQueryHandler<GetWaitingListQuery, GetWaitingListResponse>
{
    public async Task<Result<GetWaitingListResponse>> Handle(
        GetWaitingListQuery query,
        CancellationToken cancellationToken)
    {
        var baseQuery = context.Registrations
            .Include(r => r.StudentProfile)
            .Include(r => r.Branch)
            .Include(r => r.Program)
            .Include(r => r.TuitionPlan)
            .Where(r => r.Status == RegistrationStatus.New || r.Status == RegistrationStatus.WaitingForClass)
            .AsQueryable();

        if (query.BranchId.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.BranchId == query.BranchId.Value);
        }

        if (query.ProgramId.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.ProgramId == query.ProgramId.Value);
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var items = await baseQuery
            .OrderBy(r => r.ExpectedStartDate ?? r.RegistrationDate)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new WaitingListItemDto
            {
                Id = r.Id,
                StudentProfileId = r.StudentProfileId,
                StudentName = r.StudentProfile.Name != null ? r.StudentProfile.Name : string.Empty,
                BranchId = r.BranchId,
                BranchName = r.Branch.Name,
                ProgramId = r.ProgramId,
                ProgramName = r.Program.Name,
                TuitionPlanId = r.TuitionPlanId,
                TuitionPlanName = r.TuitionPlan.Name,
                RegistrationDate = r.RegistrationDate,
                ExpectedStartDate = r.ExpectedStartDate,
                PreferredSchedule = r.PreferredSchedule,
                DaysWaiting = (int)(now - r.RegistrationDate).TotalDays
            })
            .ToListAsync(cancellationToken);

        return new GetWaitingListResponse
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
