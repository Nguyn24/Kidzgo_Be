using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetRegistrationById;

public sealed class GetRegistrationByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetRegistrationByIdQuery, GetRegistrationByIdResponse>
{
    public async Task<Result<GetRegistrationByIdResponse>> Handle(
        GetRegistrationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var registration = await context.Registrations
            .Include(r => r.StudentProfile)
            .Include(r => r.Branch)
            .Include(r => r.Program)
            .Include(r => r.TuitionPlan)
            .Include(r => r.Class)
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

        if (registration == null)
        {
            return Result.Failure<GetRegistrationByIdResponse>(RegistrationErrors.NotFound(query.Id));
        }

        return new GetRegistrationByIdResponse
        {
            Id = registration.Id,
            StudentProfileId = registration.StudentProfileId,
            StudentName = registration.StudentProfile.DisplayName,
            BranchId = registration.BranchId,
            BranchName = registration.Branch.Name,
            ProgramId = registration.ProgramId,
            ProgramName = registration.Program.Name,
            TuitionPlanId = registration.TuitionPlanId,
            TuitionPlanName = registration.TuitionPlan.Name,
            RegistrationDate = registration.RegistrationDate,
            ExpectedStartDate = registration.ExpectedStartDate,
            ActualStartDate = registration.ActualStartDate,
            PreferredSchedule = registration.PreferredSchedule,
            Note = registration.Note,
            Status = registration.Status.ToString(),
            ClassId = registration.ClassId,
            ClassName = registration.Class?.Title,
            EntryType = registration.EntryType?.ToString(),
            TotalSessions = registration.TotalSessions,
            UsedSessions = registration.UsedSessions,
            RemainingSessions = registration.RemainingSessions,
            ExpiryDate = registration.ExpiryDate,
            OriginalRegistrationId = registration.OriginalRegistrationId,
            OperationType = registration.OperationType?.ToString(),
            CreatedAt = registration.CreatedAt,
            UpdatedAt = registration.UpdatedAt
        };
    }
}
