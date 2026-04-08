using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Classes.GetTeacherClasses;
using Kidzgo.Application.Classes.GetTeacherClassStudents;
using Kidzgo.Application.Classes.GetTeacherStudents;
using Kidzgo.Application.Sessions.GetTeacherTimetable;
using Kidzgo.Application.Users.GetCurrentUser;
using Kidzgo.Application.Users.GetTeacherOverview;
using Kidzgo.Application.Users.UpdateCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.API.Controllers;

[Route("api/teacher")]
[ApiController]
[Authorize]
public class TeacherController : ControllerBase
{
    private readonly IDbContext _context;
    private readonly ISender _mediator;
    private readonly IUserContext _userContext;

    public TeacherController(ISender mediator, IDbContext context, IUserContext userContext)
    {
        _mediator = mediator;
        _context = context;
        _userContext = userContext;
    }

    [HttpGet("classes")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetClasses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherClassesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("classes/{classId:guid}/students")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetClassStudents(
        Guid classId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherClassStudentsQuery
        {
            ClassId = classId,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("students")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetStudents(
        [FromQuery] Guid? classId,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherStudentsQuery
        {
            ClassId = classId,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("timetable")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> GetTimetable(
        [FromQuery] Guid? teacherUserId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? classId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherTimetableQuery
        {
            TeacherUserId = teacherUserId,
            From = from,
            To = to,
            BranchId = branchId,
            ClassId = classId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetTeacherDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTeacherOverviewQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return CustomResults.Problem(result);
        }

        return OkData(new
        {
            stats = result.Value.Statistics,
            todayClasses = result.Value.UpcomingSessions
                .Where(s => s.PlannedDatetime.Date == DateTime.UtcNow.Date)
                .ToList(),
            upcomingClasses = result.Value.UpcomingSessions,
            alerts = result.Value.OpenTickets.Take(5).ToList(),
            recentActivities = result.Value.RecentAttendances,
            pendingTasks = result.Value.PendingHomeworks.Cast<object>()
                .Concat(result.Value.PendingReports.Cast<object>())
                .ToList()
        });
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetTeacherProfile(CancellationToken cancellationToken)
    {
        var currentUserResult = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        if (!currentUserResult.IsSuccess)
        {
            return CustomResults.Problem(currentUserResult);
        }

        var overviewResult = await _mediator.Send(new GetTeacherOverviewQuery(), cancellationToken);
        if (!overviewResult.IsSuccess)
        {
            return CustomResults.Problem(overviewResult);
        }

        return OkData(new
        {
            basicInfo = new
            {
                currentUserResult.Value.Id,
                currentUserResult.Value.FullName,
                currentUserResult.Value.Email,
                currentUserResult.Value.PhoneNumber,
                currentUserResult.Value.AvatarUrl,
                currentUserResult.Value.BranchId,
                branchName = currentUserResult.Value.Branch?.Name
            },
            bio = (string?)null,
            skills = Array.Empty<string>(),
            certificates = Array.Empty<object>(),
            teachingStats = overviewResult.Value.Statistics
        });
    }

    [HttpPut("profile")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> UpdateTeacherProfile(
        [FromBody] UpdateCurrentUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCurrentUserCommand
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl,
            Profiles = request.Profiles?.Select(p => new Kidzgo.Application.Users.UpdateCurrentUser.UpdateProfileDto
            {
                Id = p.Id,
                DisplayName = p.DisplayName
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("timesheet")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> GetTeacherTimesheet(
        [FromQuery] Guid? teacherUserId,
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        var targetTeacherId = User.IsInRole("Teacher")
            ? _userContext.UserId
            : teacherUserId ?? _userContext.UserId;

        var workHoursQuery = _context.MonthlyWorkHours
            .AsNoTracking()
            .Where(m => m.StaffUserId == targetTeacherId);

        if (year.HasValue)
        {
            workHoursQuery = workHoursQuery.Where(m => m.Year == year.Value);
        }

        var monthlyData = await workHoursQuery
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .Select(m => new
            {
                month = $"{m.Year:D4}-{m.Month:D2}",
                hours = m.TotalHours,
                income = _context.PayrollPayments
                    .Where(p => p.StaffUserId == targetTeacherId && p.PaidAt != null && p.PaidAt.Value.Year == m.Year && p.PaidAt.Value.Month == m.Month)
                    .Sum(p => (decimal?)p.Amount) ?? 0,
                rate = m.Contract.HourlyRate,
                classCount = m.TeachingSessions,
                status = m.IsLocked ? "Locked" : "Open"
            })
            .ToListAsync(cancellationToken);

        return OkData(new
        {
            monthlyData,
            yearlySummary = new
            {
                totalHours = monthlyData.Sum(x => x.hours),
                totalIncome = monthlyData.Sum(x => x.income),
                averagePerMonth = monthlyData.Count == 0 ? 0 : Math.Round(monthlyData.Average(x => x.income), 2),
                totalClasses = monthlyData.Sum(x => x.classCount)
            }
        });
    }

    private static IResult OkData<T>(T data)
    {
        return Results.Ok(ApiResult<T>.Success(data));
    }
}
