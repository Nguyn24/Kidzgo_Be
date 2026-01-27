using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.PlacementTests.AddPlacementTestNote;
using Kidzgo.Application.PlacementTests.CancelPlacementTest;
using Kidzgo.Application.PlacementTests.ConvertLeadToEnrolled;
using Kidzgo.Application.PlacementTests.GetPlacementTestById;
using Kidzgo.Application.PlacementTests.GetPlacementTests;
using Kidzgo.Application.PlacementTests.MarkPlacementTestNoShow;
using Kidzgo.Application.PlacementTests.SchedulePlacementTest;
using Kidzgo.Application.PlacementTests.UpdatePlacementTest;
using Kidzgo.Application.PlacementTests.UpdatePlacementTestResults;
using Kidzgo.Domain.CRM;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

/// <summary>
/// UC-027 to UC-038: Placement Test Management APIs
/// </summary>
[Route("api/placement-tests")]
[ApiController]
[Authorize]
public class PlacementTestController : ControllerBase
{
    private readonly ISender _mediator;

    public PlacementTestController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-027: Đặt lịch Placement Test
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> SchedulePlacementTest(
        [FromBody] SchedulePlacementTestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SchedulePlacementTestCommand
        {
            LeadId = request.LeadId,
            StudentProfileId = request.StudentProfileId,
            ClassId = request.ClassId,
            ScheduledAt = request.ScheduledAt,
            Room = request.Room,
            InvigilatorUserId = request.InvigilatorUserId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-028: Xem danh sách Placement Tests
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> GetPlacementTests(
        [FromQuery] Guid? leadId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] PlacementTestStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPlacementTestsQuery
        {
            LeadId = leadId,
            StudentProfileId = studentProfileId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-028: Xem chi tiết Placement Test
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> GetPlacementTestById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetPlacementTestByIdQuery
        {
            PlacementTestId = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-029: Cập nhật thông tin Placement Test
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdatePlacementTest(
        Guid id,
        [FromBody] UpdatePlacementTestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePlacementTestCommand
        {
            PlacementTestId = id,
            ScheduledAt = request.ScheduledAt,
            Room = request.Room,
            InvigilatorUserId = request.InvigilatorUserId,
            StudentProfileId = request.StudentProfileId,
            ClassId = request.ClassId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-030: Hủy Placement Test
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CancelPlacementTest(
        Guid id,
        [FromBody] CancelPlacementTestRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new CancelPlacementTestCommand
        {
            PlacementTestId = id,
            Reason = request?.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-031: Đánh dấu NO_SHOW
    /// </summary>
    [HttpPost("{id}/no-show")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> MarkPlacementTestNoShow(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new MarkPlacementTestNoShowCommand
        {
            PlacementTestId = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-032 to UC-036: Nhập kết quả Placement Test
    /// </summary>
    [HttpPut("{id}/results")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdatePlacementTestResults(
        Guid id,
        [FromBody] UpdatePlacementTestResultsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePlacementTestResultsCommand
        {
            PlacementTestId = id,
            ListeningScore = request.ListeningScore,
            SpeakingScore = request.SpeakingScore,
            ReadingScore = request.ReadingScore,
            WritingScore = request.WritingScore,
            ResultScore = request.ResultScore,
            LevelRecommendation = request.LevelRecommendation,
            ProgramRecommendation = request.ProgramRecommendation,
            AttachmentUrl = request.AttachmentUrl
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-037: Ghi chú Placement Test
    /// </summary>
    [HttpPost("{id}/notes")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AddPlacementTestNote(
        Guid id,
        [FromBody] AddPlacementTestNoteRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddPlacementTestNoteCommand
        {
            PlacementTestId = id,
            Note = request.Note
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-038: Chuyển Lead sang ENROLLED sau Placement Test
    /// </summary>
    [HttpPost("{id}/convert-to-enrolled")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ConvertLeadToEnrolled(
        Guid id,
        [FromBody] ConvertLeadToEnrolledRequest? request,
        CancellationToken cancellationToken)
    {
        var command = new ConvertLeadToEnrolledCommand
        {
            PlacementTestId = id,
            StudentProfileId = request?.StudentProfileId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

