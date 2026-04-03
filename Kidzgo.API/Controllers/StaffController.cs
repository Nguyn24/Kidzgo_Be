using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Notifications.BroadcastNotification;
using Kidzgo.Application.Notifications.GetBroadcastNotificationHistory;
using Kidzgo.Application.Users.GetManagementStaffOverview;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.API.Controllers;

[Route("api/staff")]
[ApiController]
[Authorize(Roles = "Staff,ManagementStaff,Admin")]
public class StaffController : ControllerBase
{
    private readonly IDbContext _context;
    private readonly ISender _mediator;

    public StaffController(IDbContext context, ISender mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    [HttpGet("dashboard")]
    public async Task<IResult> GetStaffDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetManagementStaffOverviewQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return CustomResults.Problem(result);
        }

        return OkData(new
        {
            activeStudents = result.Value.Classes.Sum(c => c.EnrollmentCount),
            tuitionCollected = 0m,
            pendingRegistrations = result.Value.RecentEnrollments.Count,
            recentActivities = result.Value.PendingReports.Cast<object>()
                .Concat(result.Value.OpenTickets.Cast<object>())
                .Take(10)
                .ToList()
        });
    }

    [HttpGet("announcements/history")]
    public async Task<IResult> GetStaffAnnouncementsHistory(
        [FromQuery] string? senderRole,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetBroadcastNotificationHistoryQuery
        {
            SenderRole = senderRole,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("announcements")]
    public async Task<IResult> CreateStaffAnnouncement(
        [FromBody] BroadcastNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new BroadcastNotificationCommand
        {
            Title = request.Title,
            Content = request.Content,
            Deeplink = request.Deeplink,
            Channel = request.Channel,
            Kind = request.Kind,
            Priority = request.Priority,
            SenderRole = string.IsNullOrWhiteSpace(request.SenderRole) ? "Staff" : request.SenderRole,
            SenderName = request.SenderName,
            Role = request.Role,
            BranchId = request.BranchId,
            ClassId = request.ClassId,
            StudentProfileId = request.StudentProfileId,
            UserIds = request.UserIds,
            ProfileIds = request.ProfileIds
        }, cancellationToken);

        return result.MatchCreated(r => $"/api/staff/announcements/history/{r.Id}");
    }

    [HttpGet("students")]
    public async Task<IResult> GetStaffStudents(CancellationToken cancellationToken)
    {
        var students = await _context.Profiles
            .AsNoTracking()
            .Where(p => p.ProfileType == ProfileType.Student && !p.IsDeleted)
            .OrderBy(p => p.DisplayName)
            .Take(200)
            .Select(p => new
            {
                studentId = p.Id,
                fullName = p.DisplayName,
                phone = p.User.PhoneNumber,
                status = p.IsActive ? "Active" : "Inactive",
                course = p.ClassEnrollments
                    .Where(ce => ce.Status == Domain.Classes.EnrollmentStatus.Active)
                    .Select(ce => ce.Class.Title)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return OkData(students);
    }

    [HttpGet("enrollments/pending")]
    public async Task<IResult> GetStaffPendingEnrollments(CancellationToken cancellationToken)
    {
        var items = await _context.Registrations
            .AsNoTracking()
            .Where(r => r.Status == RegistrationStatus.New || r.Status == RegistrationStatus.WaitingForClass)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                studentName = r.StudentProfile.DisplayName,
                branchName = r.Branch.Name,
                programName = r.Program.Name,
                className = r.Class != null ? r.Class.Title : null,
                enrollDate = r.RegistrationDate,
                status = r.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return OkData(items);
    }

    [HttpPost("enrollments/{id:guid}/approve")]
    public async Task<IResult> ApproveStaffEnrollment(Guid id, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (registration is null)
        {
            return NotFoundProblem("Registration", "Registration not found");
        }

        registration.Status = registration.ClassId.HasValue ? RegistrationStatus.ClassAssigned : RegistrationStatus.WaitingForClass;
        registration.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return OkData(new { registration.Id, status = registration.Status.ToString() });
    }

    [HttpPost("enrollments/{id:guid}/reject")]
    public async Task<IResult> RejectStaffEnrollment(Guid id, CancellationToken cancellationToken)
    {
        var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (registration is null)
        {
            return NotFoundProblem("Registration", "Registration not found");
        }

        registration.Status = RegistrationStatus.Cancelled;
        registration.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return OkData(new { registration.Id, status = registration.Status.ToString() });
    }

    [HttpGet("fees/summary")]
    public async Task<IResult> GetStaffFeesSummary(CancellationToken cancellationToken)
    {
        var duesList = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue)
            .OrderBy(i => i.DueDate)
            .Take(20)
            .Select(i => new
            {
                id = i.Id,
                studentId = i.StudentProfileId,
                studentName = i.StudentProfile.DisplayName,
                className = i.Class != null ? i.Class.Title : null,
                amount = i.Amount,
                dueDate = i.DueDate,
                status = i.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var recentPayments = await _context.Payments
            .AsNoTracking()
            .Where(p => p.PaidAt != null)
            .OrderByDescending(p => p.PaidAt)
            .Take(20)
            .Select(p => new
            {
                p.Id,
                p.InvoiceId,
                studentName = p.Invoice.StudentProfile.DisplayName,
                p.Amount,
                method = p.Method.ToString(),
                paidAt = p.PaidAt
            })
            .ToListAsync(cancellationToken);

        return OkData(new
        {
            duesList,
            recentPayments,
            reconciliationExport = (string?)null
        });
    }

    private static IResult OkData<T>(T data)
    {
        return Results.Ok(ApiResult<T>.Success(data));
    }

    private static IResult NotFoundProblem(string title, string detail)
    {
        return Results.Problem(title: title, detail: detail, statusCode: StatusCodes.Status404NotFound);
    }
}
