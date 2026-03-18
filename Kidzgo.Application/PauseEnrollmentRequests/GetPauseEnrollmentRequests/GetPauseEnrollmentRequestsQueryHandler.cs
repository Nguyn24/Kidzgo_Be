using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.PauseEnrollmentRequests;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.GetPauseEnrollmentRequests;

public sealed class GetPauseEnrollmentRequestsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetPauseEnrollmentRequestsQuery, Page<PauseEnrollmentRequestResponse>>
{
    public async Task<Result<Page<PauseEnrollmentRequestResponse>>> Handle(
        GetPauseEnrollmentRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var studentProfileId = request.StudentProfileId ?? userContext.StudentId;

        var query = context.PauseEnrollmentRequests.AsQueryable();

        if (studentProfileId.HasValue)
        {
            query = query.Where(r => r.StudentProfileId == studentProfileId.Value);
        }

        if (request.ClassId.HasValue)
        {
            query = query.Where(r => r.ClassId == request.ClassId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

        if (request.BranchId.HasValue)
        {
            query = query.Where(r => r.Class.BranchId == request.BranchId.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.RequestedAt)
            .ApplyPagination(request.PageNumber, request.PageSize)
            .Select(r => new
            {
                r.Id,
                r.StudentProfileId,
                r.ClassId,
                r.PauseFrom,
                r.PauseTo,
                r.Reason,
                r.Status,
                r.RequestedAt,
                r.ApprovedBy,
                r.ApprovedAt,
                r.CancelledBy,
                r.CancelledAt,
                r.Outcome,
                r.OutcomeNote,
                r.OutcomeBy,
                r.OutcomeAt
            })
            .ToListAsync(cancellationToken);

        var requestIds = items.Select(r => r.Id).ToList();

        var classRows = await (
            from r in context.PauseEnrollmentRequests
            join e in context.ClassEnrollments on r.StudentProfileId equals e.StudentProfileId
            join s in context.Sessions on e.ClassId equals s.ClassId
            join c in context.Classes on e.ClassId equals c.Id
            where requestIds.Contains(r.Id)
                  && e.Status != EnrollmentStatus.Dropped
                  && DateOnly.FromDateTime(s.PlannedDatetime) >= r.PauseFrom
                  && DateOnly.FromDateTime(s.PlannedDatetime) <= r.PauseTo
            select new
            {
                RequestId = r.Id,
                ClassId = c.Id,
                c.Code,
                c.Title,
                c.ProgramId,
                ProgramName = c.Program.Name,
                c.BranchId,
                BranchName = c.Branch.Name,
                c.StartDate,
                c.EndDate,
                Status = c.Status.ToString()
            })
            .Distinct()
            .ToListAsync(cancellationToken);

        var classLookup = classRows
            .GroupBy(r => r.RequestId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(c => new PauseEnrollmentClassDto
                {
                    Id = c.ClassId,
                    Code = c.Code,
                    Title = c.Title,
                    ProgramId = c.ProgramId,
                    ProgramName = c.ProgramName,
                    BranchId = c.BranchId,
                    BranchName = c.BranchName,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    Status = c.Status
                }).ToList());

        var responses = items
            .Select(r => new PauseEnrollmentRequestResponse
            {
                Id = r.Id,
                StudentProfileId = r.StudentProfileId,
                ClassId = r.ClassId,
                PauseFrom = r.PauseFrom,
                PauseTo = r.PauseTo,
                Reason = r.Reason,
                Status = r.Status.ToString(),
                RequestedAt = r.RequestedAt,
                ApprovedBy = r.ApprovedBy,
                ApprovedAt = r.ApprovedAt,
                CancelledBy = r.CancelledBy,
                CancelledAt = r.CancelledAt,
                Outcome = r.Outcome.HasValue ? r.Outcome.Value.ToString() : null,
                OutcomeNote = r.OutcomeNote,
                OutcomeBy = r.OutcomeBy,
                OutcomeAt = r.OutcomeAt,
                Classes = classLookup.TryGetValue(r.Id, out var classes) ? classes : new List<PauseEnrollmentClassDto>()
            })
            .ToList();

        return new Page<PauseEnrollmentRequestResponse>(responses, total, request.PageNumber, request.PageSize);
    }
}
