using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.GetEnrollmentById;

public sealed class GetEnrollmentByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetEnrollmentByIdQuery, GetEnrollmentByIdResponse>
{
    public async Task<Result<GetEnrollmentByIdResponse>> Handle(GetEnrollmentByIdQuery query, CancellationToken cancellationToken)
    {
        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.Class)
                .ThenInclude(c => c.Branch)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .FirstOrDefaultAsync(e => e.Id == query.Id, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<GetEnrollmentByIdResponse>(
                EnrollmentErrors.NotFound(query.Id));
        }

        return new GetEnrollmentByIdResponse
        {
            Id = enrollment.Id,
            ClassId = enrollment.ClassId,
            ClassCode = enrollment.Class.Code,
            ClassTitle = enrollment.Class.Title,
            ProgramId = enrollment.Class.ProgramId,
            ProgramName = enrollment.Class.Program.Name,
            BranchId = enrollment.Class.BranchId,
            BranchName = enrollment.Class.Branch.Name,
            StudentProfileId = enrollment.StudentProfileId,
            StudentName = enrollment.StudentProfile.DisplayName,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status,
            TuitionPlanId = enrollment.TuitionPlanId,
            TuitionPlanName = enrollment.TuitionPlan?.Name,
            CreatedAt = enrollment.CreatedAt,
            UpdatedAt = enrollment.UpdatedAt
        };
    }
}

