using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportRequests.CompleteReportRequest;

public sealed class CompleteReportRequestCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CompleteReportRequestCommand, ReportRequestDto>
{
    public async Task<Result<ReportRequestDto>> Handle(
        CompleteReportRequestCommand command,
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

        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<ReportRequestDto>(
                Error.NotFound("User.NotFound", "User was not found"));
        }

        if (currentUser.Role == UserRole.Teacher && request.AssignedTeacherUserId != currentUser.Id)
        {
            return Result.Failure<ReportRequestDto>(
                Error.Validation("ReportRequest.Unauthorized", "You can only complete your own report requests"));
        }

        if (request.Status != ReportRequestStatus.Requested &&
            request.Status != ReportRequestStatus.InProgress)
        {
            return Result.Failure<ReportRequestDto>(
                ReportRequestErrors.InvalidStatus(request.Status, "complete"));
        }

        if (command.LinkedSessionReportId.HasValue)
        {
            var validation = await ValidateSessionReportLinkAsync(
                request,
                command.LinkedSessionReportId.Value,
                cancellationToken);

            if (validation.IsFailure)
            {
                return Result.Failure<ReportRequestDto>(validation.Error);
            }

            request.LinkedSessionReportId = command.LinkedSessionReportId.Value;
        }

        if (command.LinkedMonthlyReportId.HasValue)
        {
            var validation = await ValidateMonthlyReportLinkAsync(
                request,
                command.LinkedMonthlyReportId.Value,
                cancellationToken);

            if (validation.IsFailure)
            {
                return Result.Failure<ReportRequestDto>(validation.Error);
            }

            request.LinkedMonthlyReportId = command.LinkedMonthlyReportId.Value;
        }

        var now = DateTime.UtcNow;
        request.Status = ReportRequestStatus.Submitted;
        request.SubmittedAt = now;
        request.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return ReportRequestMapper.ToDto(request);
    }

    private async Task<Result> ValidateSessionReportLinkAsync(
        ReportRequest request,
        Guid sessionReportId,
        CancellationToken cancellationToken)
    {
        if (request.ReportType != ReportRequestType.Session)
        {
            return Result.Failure(
                Error.Validation("ReportRequest.InvalidReportLink", "Cannot link a session report to a monthly report request"));
        }

        var report = await context.SessionReports
            .Include(r => r.Session)
            .FirstOrDefaultAsync(r => r.Id == sessionReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure(SessionReportErrors.NotFound(sessionReportId));
        }

        if (report.TeacherUserId != request.AssignedTeacherUserId ||
            (request.TargetStudentProfileId.HasValue && report.StudentProfileId != request.TargetStudentProfileId.Value) ||
            (request.TargetSessionId.HasValue && report.SessionId != request.TargetSessionId.Value) ||
            (request.TargetClassId.HasValue && report.Session.ClassId != request.TargetClassId.Value))
        {
            return Result.Failure(
                Error.Validation("ReportRequest.ReportMismatch", "Linked report does not match this request"));
        }

        return Result.Success();
    }

    private async Task<Result> ValidateMonthlyReportLinkAsync(
        ReportRequest request,
        Guid monthlyReportId,
        CancellationToken cancellationToken)
    {
        if (request.ReportType != ReportRequestType.Monthly)
        {
            return Result.Failure(
                Error.Validation("ReportRequest.InvalidReportLink", "Cannot link a monthly report to a session report request"));
        }

        var report = await context.StudentMonthlyReports
            .FirstOrDefaultAsync(r => r.Id == monthlyReportId, cancellationToken);

        if (report is null)
        {
            return Result.Failure(MonthlyReportErrors.NotFound(monthlyReportId));
        }

        if ((request.TargetStudentProfileId.HasValue && report.StudentProfileId != request.TargetStudentProfileId.Value) ||
            (request.TargetClassId.HasValue && report.ClassId != request.TargetClassId.Value) ||
            (request.Month.HasValue && report.Month != request.Month.Value) ||
            (request.Year.HasValue && report.Year != request.Year.Value))
        {
            return Result.Failure(
                Error.Validation("ReportRequest.ReportMismatch", "Linked report does not match this request"));
        }

        return Result.Success();
    }
}
