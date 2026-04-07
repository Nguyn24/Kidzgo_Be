using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportRequests.CancelReportRequest;

public sealed class CancelReportRequestCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CancelReportRequestCommand, ReportRequestDto>
{
    public async Task<Result<ReportRequestDto>> Handle(
        CancelReportRequestCommand command,
        CancellationToken cancellationToken)
    {
        var request = await context.ReportRequests
            .Include(r => r.AssignedTeacher)
            .Include(r => r.RequestedByUser)
            .Include(r => r.TargetStudentProfile)
            .Include(r => r.TargetClass)
            .Include(r => r.TargetSession)
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (request is null)
        {
            return Result.Failure<ReportRequestDto>(ReportRequestErrors.NotFound(command.Id));
        }

        if (request.Status is ReportRequestStatus.Approved or ReportRequestStatus.Cancelled)
        {
            return Result.Failure<ReportRequestDto>(
                ReportRequestErrors.InvalidStatus(request.Status, "cancel"));
        }

        var requesterExists = await context.Users
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (!requesterExists)
        {
            return Result.Failure<ReportRequestDto>(
                Error.NotFound("User.NotFound", "User was not found"));
        }

        request.Status = ReportRequestStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return ReportRequestMapper.ToDto(request);
    }
}
