using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetRegistrations;

public sealed class GetRegistrationsQueryHandler(
    IDbContext context
) : IQueryHandler<GetRegistrationsQuery, GetRegistrationsResponse>
{
    public async Task<Result<GetRegistrationsResponse>> Handle(
        GetRegistrationsQuery query,
        CancellationToken cancellationToken)
    {
        var baseQuery = context.Registrations
            .Include(r => r.StudentProfile)
            .Include(r => r.Branch)
            .Include(r => r.Program)
            .Include(r => r.TuitionPlan)
            .Include(r => r.Class)
            .AsQueryable();

        // Apply filters
        if (query.StudentProfileId.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.BranchId.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.BranchId == query.BranchId.Value);
        }

        if (query.ProgramId.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.ProgramId == query.ProgramId.Value);
        }

        if (query.ClassId.HasValue)
        {
            baseQuery = baseQuery.Where(r => r.ClassId == query.ClassId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (Enum.TryParse<RegistrationStatus>(query.Status, true, out var status))
            {
                baseQuery = baseQuery.Where(r => r.Status == status);
            }
        }

        // Get total count
        var totalCount = await baseQuery.CountAsync(cancellationToken);

        // Apply pagination
        var items = await baseQuery
            .OrderByDescending(r => r.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new RegistrationDto
            {
                Id = r.Id,
                StudentProfileId = r.StudentProfileId,
                StudentName = r.StudentProfile.DisplayName,
                BranchId = r.BranchId,
                BranchName = r.Branch.Name,
                ProgramId = r.ProgramId,
                ProgramName = r.Program.Name,
                TuitionPlanId = r.TuitionPlanId,
                TuitionPlanName = r.TuitionPlan.Name,
                RegistrationDate = r.RegistrationDate,
                ExpectedStartDate = r.ExpectedStartDate,
                ActualStartDate = r.ActualStartDate,
                PreferredSchedule = r.PreferredSchedule,
                Note = r.Note,
                Status = r.Status.ToString(),
                ClassId = r.ClassId,
                ClassName = r.Class != null ? r.Class.Title : null,
                TotalSessions = r.TotalSessions,
                UsedSessions = r.UsedSessions,
                RemainingSessions = r.RemainingSessions,
                ExpiryDate = r.ExpiryDate,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<RegistrationDto>(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount);

        return new GetRegistrationsResponse
        {
            Page = page
        };
    }
}
