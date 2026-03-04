using Kidzgo.API.Extensions;
using Kidzgo.Application.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly ISender _mediator;

    public DashboardController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get dashboard data with filters
    /// </summary>
    /// <param name="branchId">Filter by branch ID</param>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    [HttpGet("overall")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff,Teacher")]
    public async Task<IResult> GetDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
    
    [HttpGet("student")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff,Teacher")]
    public async Task<IResult> GetStudentDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.Map(r => r.Students).MatchOk();
    }

    /// <summary>
    /// Get academic dashboard (attendance, homework, leave, makeup)
    /// </summary>
    [HttpGet("academic")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetAcademicDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        
        return result.Map(r => new
        {
            r.Attendance,
            r.Homework,
            r.Leave,
            r.MakeupCredits
        }).MatchOk();
    }

    /// <summary>
    /// Get financial dashboard (revenue, outstanding debt)
    /// </summary>
    [HttpGet("finance")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> GetFinanceDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        
        return result.Map(r => r.Finance).MatchOk();
    }

    /// <summary>
    /// Get HR dashboard (staff, work hours, payroll)
    /// </summary>
    [HttpGet("hr")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetHrDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        
        return result.Map(r => r.HumanResources).MatchOk();
    }

    /// <summary>
    /// Get CRM dashboard (leads, placement tests)
    /// </summary>
    [HttpGet("leads")]
    [Authorize(Roles = "Admin,ManagementStaff,SalesStaff")]
    public async Task<IResult> GetCrmDashboard(
        [FromQuery] Guid? branchId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery
        {
            BranchId = branchId,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        
        return result.Map(r => new
        {
            r.Leads,
            r.PlacementTests
        }).MatchOk();
    }
}

