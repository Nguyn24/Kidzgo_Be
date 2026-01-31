using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Leads.AddLeadNote;
using Kidzgo.Application.Leads.AssignLead;
using Kidzgo.Application.Leads.CreateLead;
using Kidzgo.Application.Leads.GetLeadActivities;
using Kidzgo.Application.Leads.GetLeadById;
using Kidzgo.Application.Leads.GetLeadSLA;
using Kidzgo.Application.Leads.GetLeads;
using Kidzgo.Application.Leads.UpdateLead;
using Kidzgo.Application.Leads.UpdateLeadStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

/// <summary>
/// UC-013 to UC-026: Lead Management APIs
/// </summary>
[Route("api/leads")]
[ApiController]
[Authorize]
public class LeadController : ControllerBase
{
    private readonly ISender _mediator;

    public LeadController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-013: Tạo Lead từ form web (Public endpoint - không cần auth)
    /// </summary>
    [HttpPost("public")]
    [AllowAnonymous]
    public async Task<IResult> CreateLeadFromWeb(
        [FromBody] PublicCreateLeadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLeadCommand
        {
            Source = Kidzgo.Domain.CRM.LeadSource.Landing, // Tự động set source = Landing cho form web
            ContactName = request.ContactName,
            Phone = request.Phone,
            ZaloId = request.ZaloId,
            Email = request.Email,
            OwnerStaffId = null // Không gán owner khi tạo từ form web
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(l => $"/api/leads/{l.Id}");
    }

    /// <summary>
    /// UC-014-016: Tạo Lead từ Zalo/Referral/Offline (Internal - cần auth)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> CreateLead(
        [FromBody] CreateLeadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLeadCommand
        {
            Source = request.Source,
            Campaign = request.Campaign,
            ContactName = request.ContactName,
            ChildName = request.ChildName,
            ChildDateOfBirth = request.ChildDateOfBirth,
            Phone = request.Phone,
            ZaloId = request.ZaloId,
            Email = request.Email,
            Company = request.Company,
            Subject = request.Subject,
            BranchPreference = request.BranchPreference,
            ProgramInterest = request.ProgramInterest,
            Notes = request.Notes,
            OwnerStaffId = request.OwnerStaffId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(l => $"/api/leads/{l.Id}");
    }

    /// <summary>
    /// UC-017: Xem danh sách Leads
    /// </summary>
    /// <param name="status">Status: New, Contacted, BookedTest, TestDone, Enrolled, Lost</param>
    /// <param name="source">Source: Landing, Zalo, Referral, Offline</param>
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> GetLeads(
        [FromQuery] string? status,
        [FromQuery] string? source,
        [FromQuery] Guid? ownerStaffId,
        [FromQuery] Guid? branchPreference,
        [FromQuery] string? searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLeadsQuery
        {
            Status = !string.IsNullOrWhiteSpace(status) && Enum.TryParse<Kidzgo.Domain.CRM.LeadStatus>(status, true, out var statusEnum)
                ? statusEnum
                : null,
            Source = !string.IsNullOrWhiteSpace(source) && Enum.TryParse<Kidzgo.Domain.CRM.LeadSource>(source, true, out var sourceEnum)
                ? sourceEnum
                : null,
            OwnerStaffId = ownerStaffId,
            BranchPreference = branchPreference,
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-018: Xem chi tiết Lead
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> GetLeadById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetLeadByIdQuery
        {
            LeadId = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-019: Cập nhật thông tin Lead
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> UpdateLead(
        Guid id,
        [FromBody] UpdateLeadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLeadCommand
        {
            LeadId = id,
            ContactName = request.ContactName,
            ChildName = request.ChildName,
            ChildDateOfBirth = request.ChildDateOfBirth,
            Phone = request.Phone,
            ZaloId = request.ZaloId,
            Email = request.Email,
            Company = request.Company,
            Subject = request.Subject,
            BranchPreference = request.BranchPreference,
            ProgramInterest = request.ProgramInterest,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-020: Gán Lead cho Staff
    /// </summary>
    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AssignLead(
        Guid id,
        [FromBody] AssignLeadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignLeadCommand
        {
            LeadId = id,
            OwnerStaffId = request.OwnerStaffId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-021: Cập nhật trạng thái Lead
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> UpdateLeadStatus(
        Guid id,
        [FromBody] UpdateLeadStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLeadStatusCommand
        {
            LeadId = id,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-022: Ghi chú Lead
    /// </summary>
    [HttpPost("{id:guid}/notes")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> AddLeadNote(
        Guid id,
        [FromBody] AddLeadNoteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddLeadNoteCommand
        {
            LeadId = id,
            Content = request.Content,
            ActivityType = request.ActivityType,
            NextActionAt = request.NextActionAt
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-023: Xem lịch sử hoạt động Lead
    /// </summary>
    [HttpGet("{id:guid}/activities")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> GetLeadActivities(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetLeadActivitiesQuery
        {
            LeadId = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-024: Theo dõi SLA phản hồi đầu tiên
    /// </summary>
    [HttpGet("{id:guid}/sla")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> GetLeadSLA(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetLeadSLAQuery
        {
            LeadId = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Lấy danh sách tất cả các status có thể có của Lead
    /// </summary>
    [HttpGet("statuses")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public IResult GetLeadStatuses()
    {
        var statuses = Enum.GetValues(typeof(Kidzgo.Domain.CRM.LeadStatus))
            .Cast<Kidzgo.Domain.CRM.LeadStatus>()
            .Select(s => s.ToString())
            .ToList();

        return Results.Ok(new { statuses });
    }
}

