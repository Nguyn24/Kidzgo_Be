using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportRequests.GetReportRequestById;

public sealed class GetReportRequestByIdQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetReportRequestByIdQuery, ReportRequestDto>
{
    public async Task<Result<ReportRequestDto>> Handle(
        GetReportRequestByIdQuery query,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<ReportRequestDto>(
                Error.NotFound("User.NotFound", "User was not found"));
        }

        var request = await context.ReportRequests
            .Include(r => r.AssignedTeacher)
            .Include(r => r.RequestedByUser)
            .Include(r => r.TargetStudentProfile)
            .Include(r => r.TargetClass)
            .Include(r => r.TargetSession)
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken);

        if (request is null)
        {
            return Result.Failure<ReportRequestDto>(ReportRequestErrors.NotFound(query.Id));
        }

        if (currentUser.Role == UserRole.Teacher && request.AssignedTeacherUserId != currentUser.Id)
        {
            return Result.Failure<ReportRequestDto>(
                Error.Validation("ReportRequest.Unauthorized", "You can only view your own report requests"));
        }

        return ReportRequestMapper.ToDto(request);
    }
}
