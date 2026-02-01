using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.LessonPlans.CreateLessonPlan;
using Kidzgo.Application.LessonPlans.DeleteLessonPlan;
using Kidzgo.Application.LessonPlans.GetLessonPlanById;
using Kidzgo.Application.LessonPlans.GetLessonPlans;
using Kidzgo.Application.LessonPlans.GetLessonPlanTemplate;
using Kidzgo.Application.LessonPlans.UpdateLessonPlan;
using Kidzgo.Application.LessonPlans.UpdateLessonPlanActual;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/lesson-plans")]
[ApiController]
[Authorize]
public class LessonPlanController : ControllerBase
{
    private readonly ISender _mediator;

    public LessonPlanController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tạo Lesson Plan mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateLessonPlan(
        [FromBody] CreateLessonPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLessonPlanCommand
        {
            SessionId = request.SessionId,
            TemplateId = request.TemplateId,
            PlannedContent = request.PlannedContent,
            ActualContent = request.ActualContent,
            ActualHomework = request.ActualHomework,
            TeacherNotes = request.TeacherNotes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/lesson-plans/{r.Id}");
    }

    /// <summary>
    /// Lấy Lesson Plan theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetLessonPlanById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetLessonPlanByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Lấy danh sách Lesson Plans với filter và pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetLessonPlans(
        [FromQuery] Guid? sessionId,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? templateId,
        [FromQuery] Guid? submittedBy,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLessonPlansQuery
        {
            SessionId = sessionId,
            ClassId = classId,
            TemplateId = templateId,
            SubmittedBy = submittedBy,
            FromDate = fromDate,
            ToDate = toDate,
            IncludeDeleted = includeDeleted,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Cập nhật Lesson Plan
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> UpdateLessonPlan(
        Guid id,
        [FromBody] UpdateLessonPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLessonPlanCommand
        {
            Id = id,
            TemplateId = request.TemplateId,
            PlannedContent = request.PlannedContent,
            ActualContent = request.ActualContent,
            ActualHomework = request.ActualHomework,
            TeacherNotes = request.TeacherNotes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Xóa Lesson Plan
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> DeleteLessonPlan(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteLessonPlanCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Lấy Giáo án khung (Read-only) - Template và PlannedContent
    /// </summary>
    [HttpGet("{id:guid}/template")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetLessonPlanTemplate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetLessonPlanTemplateQuery
        {
            LessonPlanId = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Cập nhật nội dung thực tế dạy (PATCH) - ActualContent, ActualHomework, TeacherNotes
    /// </summary>
    [HttpPatch("{id:guid}/actual")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> UpdateLessonPlanActual(
        Guid id,
        [FromBody] UpdateLessonPlanActualRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLessonPlanActualCommand
        {
            Id = id,
            ActualContent = request.ActualContent,
            ActualHomework = request.ActualHomework,
            TeacherNotes = request.TeacherNotes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

