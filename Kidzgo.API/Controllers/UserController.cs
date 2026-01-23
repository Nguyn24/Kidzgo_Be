using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Authentication.Logout;
using Kidzgo.Application.Users.GetCurrentUser;
using Kidzgo.Application.Users.UpdateCurrentUser;
using Kidzgo.Application.Users.GetAdminOverview;
using Kidzgo.Application.Users.GetTeacherOverview;
using Kidzgo.Application.Users.GetStaffOverview;
using Kidzgo.Application.Users.GetParentOverview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/me")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ISender _mediator;

    public UserController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// Get current user information with role, branchId, permissions, and selected profile
    [HttpGet]
    public async Task<IResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return result.MatchOk();
    }

    /// Update current user information and profiles
    [HttpPut]
    public async Task<IResult> UpdateCurrentUser(
        [FromBody] UpdateCurrentUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCurrentUserCommand
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl,
            Profiles = request.Profiles?.Select(p => new UpdateProfileDto
            {
                Id = p.Id,
                DisplayName = p.DisplayName
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Logout - remove all refresh tokens
    [HttpPost("logout")]
    public async Task<IResult> Logout(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LogoutCommand(), cancellationToken);
        return result.MatchOk();
    }

    /// Get Admin Overview - Dashboard data for Admin role
    [HttpGet("admin/overview")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> GetAdminOverview(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? programId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = new GetAdminOverviewQuery
        {
            BranchId = branchId,
            ClassId = classId,
            StudentProfileId = studentProfileId,
            ProgramId = programId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Get Teacher Overview - Dashboard data for Teacher role
    [HttpGet("teacher/overview")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetTeacherOverview(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? sessionId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = new GetTeacherOverviewQuery
        {
            ClassId = classId,
            StudentProfileId = studentProfileId,
            SessionId = sessionId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Get Staff Overview - Dashboard data for Staff role
    [HttpGet("staff/overview")]
    [Authorize(Roles = "Staff")]
    public async Task<IResult> GetStaffOverview(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? leadId,
        [FromQuery] Guid? enrollmentId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = new GetStaffOverviewQuery
        {
            ClassId = classId,
            StudentProfileId = studentProfileId,
            LeadId = leadId,
            EnrollmentId = enrollmentId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Get Parent Overview - Dashboard data for Parent role
    [HttpGet("parent/overview")]
    [Authorize]
    public async Task<IResult> GetParentOverview(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sessionId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = new GetParentOverviewQuery
        {
            StudentProfileId = studentProfileId,
            ClassId = classId,
            SessionId = sessionId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

