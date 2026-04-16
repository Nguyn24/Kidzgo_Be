using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.SessionReports;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.SessionReports.CreateSessionReport;

public sealed class CreateSessionReportCommandHandler(
    IDbContext context,
    IUserContext userContext,
    SessionParticipantService sessionParticipantService
) : ICommandHandler<CreateSessionReportCommand, CreateSessionReportResponse>
{
    public async Task<Result<CreateSessionReportResponse>> Handle(
        CreateSessionReportCommand command,
        CancellationToken cancellationToken)
    {
        // Verify session exists
        var session = await context.Sessions
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<CreateSessionReportResponse>(
                SessionErrors.NotFound(command.SessionId));
        }

        var now = VietnamTime.UtcNow();
        var sessionEndedCheck = SessionReportScheduleGuard.EnsureSessionHasEnded(session, now);
        if (sessionEndedCheck.IsFailure)
        {
            return Result.Failure<CreateSessionReportResponse>(sessionEndedCheck.Error);
        }

        // Verify student profile exists
        var studentProfile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId && p.ProfileType == ProfileType.Student, cancellationToken);

        if (studentProfile is null)
        {
            return Result.Failure<CreateSessionReportResponse>(
                UserErrors.NotFound(command.StudentProfileId));
        }

        // Get current teacher user ID from context
        var teacherUserId = userContext.UserId;

        // Verify teacher user exists and is a teacher
        var teacherUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == teacherUserId && u.Role == UserRole.Teacher, cancellationToken);

        if (teacherUser is null)
        {
            return Result.Failure<CreateSessionReportResponse>(
                UserErrors.NotFound(teacherUserId));
        }

        // Verify teacher is assigned to this session (either planned or actual)
        if (session.PlannedTeacherId != teacherUserId && session.ActualTeacherId != teacherUserId)
        {
            return Result.Failure<CreateSessionReportResponse>(
                SessionErrors.UnauthorizedAccess(session.Id));
        }

        var assignmentCheck = await sessionParticipantService
            .EnsureStudentAssignedToSessionAsync(command.SessionId, command.StudentProfileId, cancellationToken);

        if (assignmentCheck.IsFailure)
        {
            return Result.Failure<CreateSessionReportResponse>(assignmentCheck.Error);
        }

        // Check if report already exists for this session and student
        var existingReport = await context.SessionReports
            .FirstOrDefaultAsync(
                sr => sr.SessionId == command.SessionId && sr.StudentProfileId == command.StudentProfileId,
                cancellationToken);

        if (existingReport is not null)
        {
            return Result.Failure<CreateSessionReportResponse>(SessionReportErrors.AlreadyExists);
        }

        var sessionReport = new SessionReport
        {
            Id = Guid.NewGuid(),
            SessionId = command.SessionId,
            StudentProfileId = command.StudentProfileId,
            TeacherUserId = teacherUserId,
            ReportDate = command.ReportDate,
            Feedback = command.Feedback,
            Status = ReportStatus.Draft,
            IsMonthlyCompiled = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.SessionReports.Add(sessionReport);
        await context.SaveChangesAsync(cancellationToken);

        // Load with navigation properties for response
        var reportWithNav = await context.SessionReports
            .Include(sr => sr.Session)
                .ThenInclude(s => s.Class)
            .Include(sr => sr.StudentProfile)
            .Include(sr => sr.TeacherUser)
            .FirstOrDefaultAsync(sr => sr.Id == sessionReport.Id, cancellationToken);

        return new CreateSessionReportResponse
        {
            Id = reportWithNav!.Id,
            SessionId = reportWithNav.SessionId,
            SessionDate = reportWithNav.Session.PlannedDatetime,
            StudentProfileId = reportWithNav.StudentProfileId,
            StudentName = reportWithNav.StudentProfile.DisplayName,
            TeacherUserId = reportWithNav.TeacherUserId,
            TeacherName = reportWithNav.TeacherUser.Name,
            ReportDate = reportWithNav.ReportDate,
            Feedback = reportWithNav.Feedback,
            Status = reportWithNav.Status,
            IsMonthlyCompiled = reportWithNav.IsMonthlyCompiled,
            CreatedAt = reportWithNav.CreatedAt,
            UpdatedAt = reportWithNav.UpdatedAt
        };
    }
}
