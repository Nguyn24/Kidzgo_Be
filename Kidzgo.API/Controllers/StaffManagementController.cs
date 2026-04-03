using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Media.ApproveMedia;
using Kidzgo.Application.Media.RejectMedia;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.API.Controllers;

[Route("api/staff-management")]
[ApiController]
[Authorize(Roles = "ManagementStaff,Admin")]
public class StaffManagementController : ControllerBase
{
    private readonly IDbContext _context;
    private readonly ISender _mediator;

    public StaffManagementController(IDbContext context, ISender mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    [HttpGet("students")]
    public async Task<IResult> GetStaffManagementStudents(CancellationToken cancellationToken)
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
                className = p.ClassEnrollments
                    .Where(ce => ce.Status == Domain.Classes.EnrollmentStatus.Active)
                    .Select(ce => ce.Class.Title)
                    .FirstOrDefault(),
                attendanceRate = p.Attendances.Count == 0
                    ? 0
                    : Math.Round((decimal)p.Attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present) * 100 / p.Attendances.Count, 2),
                makeupCount = p.MakeupCredits.Count,
                notes = (string?)null,
                email = p.User.Email,
                phone = p.User.PhoneNumber,
                status = p.IsActive ? "Active" : "Inactive"
            })
            .ToListAsync(cancellationToken);

        return OkData(students);
    }

    [HttpGet("media")]
    public async Task<IResult> GetStaffManagementMedia(CancellationToken cancellationToken)
    {
        var items = await _context.MediaAssets
            .AsNoTracking()
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Take(100)
            .Select(m => new
            {
                id = m.Id,
                title = m.Caption ?? m.OriginalFileName ?? m.Url,
                className = m.Class != null ? m.Class.Title : null,
                month = m.MonthTag,
                status = m.ApprovalStatus.ToString(),
                type = m.Type.ToString(),
                uploader = m.UploaderUser.Name,
                uploadDate = m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return OkData(items);
    }

    [HttpPost("media/{id:guid}/approve")]
    public async Task<IResult> ApproveStaffManagementMedia(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ApproveMediaCommand(id), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("media/{id:guid}/reject")]
    public async Task<IResult> RejectStaffManagementMedia(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RejectMediaCommand(id), cancellationToken);
        return result.MatchOk();
    }

    private static IResult OkData<T>(T data)
    {
        return Results.Ok(ApiResult<T>.Success(data));
    }
}
