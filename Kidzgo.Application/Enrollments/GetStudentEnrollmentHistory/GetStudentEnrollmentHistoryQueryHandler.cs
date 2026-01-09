using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.GetStudentEnrollmentHistory;

public sealed class GetStudentEnrollmentHistoryQueryHandler(
    IDbContext context
) : IQueryHandler<GetStudentEnrollmentHistoryQuery, GetStudentEnrollmentHistoryResponse>
{
    public async Task<Result<GetStudentEnrollmentHistoryResponse>> Handle(GetStudentEnrollmentHistoryQuery query, CancellationToken cancellationToken)
    {
        // Check if student profile exists
        var studentProfile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == query.StudentProfileId && p.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);

        if (studentProfile is null)
        {
            return Result.Failure<GetStudentEnrollmentHistoryResponse>(
                Error.NotFound("Enrollment.StudentNotFound", "Student profile not found or is not a student"));
        }

        var enrollmentsQuery = context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.Class)
                .ThenInclude(c => c.Branch)
            .Include(e => e.TuitionPlan)
            .Where(e => e.StudentProfileId == query.StudentProfileId)
            .AsQueryable();

        // Get total count
        int totalCount = await enrollmentsQuery.CountAsync(cancellationToken);

        // Apply pagination - order by enroll date descending (most recent first)
        var enrollments = await enrollmentsQuery
            .OrderByDescending(e => e.EnrollDate)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(e => new EnrollmentHistoryDto
            {
                Id = e.Id,
                ClassId = e.ClassId,
                ClassCode = e.Class.Code,
                ClassTitle = e.Class.Title,
                ProgramId = e.Class.ProgramId,
                ProgramName = e.Class.Program.Name,
                BranchId = e.Class.BranchId,
                BranchName = e.Class.Branch.Name,
                EnrollDate = e.EnrollDate,
                Status = e.Status,
                TuitionPlanId = e.TuitionPlanId,
                TuitionPlanName = e.TuitionPlan != null ? e.TuitionPlan.Name : null,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<EnrollmentHistoryDto>(
            enrollments,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetStudentEnrollmentHistoryResponse
        {
            StudentProfileId = query.StudentProfileId,
            StudentName = studentProfile.DisplayName,
            Enrollments = page
        };
    }
}

