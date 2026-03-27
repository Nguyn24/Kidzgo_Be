using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.PlacementTests.AddPlacementTestNote;
using Kidzgo.Application.PlacementTests.CancelPlacementTest;
using Kidzgo.Application.PlacementTests.ConvertLeadToEnrolled;
using Kidzgo.Application.PlacementTests.GetPlacementTestById;
using Kidzgo.Application.PlacementTests.GetPlacementTests;
using Kidzgo.Application.PlacementTests.MarkPlacementTestNoShow;
using Kidzgo.Application.PlacementTests.RetakePlacementTest;
using Kidzgo.Application.PlacementTests.SchedulePlacementTest;
using Kidzgo.Application.PlacementTests.UpdatePlacementTest;
using Kidzgo.Application.PlacementTests.UpdatePlacementTestResults;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.CRM;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

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

    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> SchedulePlacementTest(
        [FromBody] SchedulePlacementTestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SchedulePlacementTestCommand
        {
            LeadId = request.LeadId,
            LeadChildId = request.LeadChildId,
            ScheduledAt = request.ScheduledAt,
            Room = request.Room,
            InvigilatorUserId = request.InvigilatorUserId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff,AccountantStaff")]
    public async Task<IResult> GetPlacementTests(
        [FromQuery] Guid? leadId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] PlacementTestStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? sortBy,
        [FromQuery] SortOrder sortOrder = SortOrder.Descending,
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
            SortBy = sortBy,
            SortOrder = sortOrder,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

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
            ProgramRecommendation = request.ProgramRecommendation,
            AttachmentUrl = request.AttachmentUrl
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

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

    [HttpPost("{id}/retake")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> RetakePlacementTest(
        Guid id,
        [FromBody] RetakePlacementTestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RetakePlacementTestCommand
        {
            OriginalPlacementTestId = id,
            StudentProfileId = request.StudentProfileId,
            NewProgramId = request.NewProgramId,
            NewTuitionPlanId = request.NewTuitionPlanId,
            BranchId = request.BranchId,
            ScheduledAt = request.ScheduledAt,
            Room = request.Room,
            InvigilatorUserId = request.InvigilatorUserId,
            Note = request.Note
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
