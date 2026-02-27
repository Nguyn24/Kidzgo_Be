using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.LessonPlanTemplates.CreateLessonPlanTemplate;
using Kidzgo.Application.LessonPlanTemplates.DeleteLessonPlanTemplate;
using Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplateById;
using Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplates;
using Kidzgo.Application.LessonPlanTemplates.UpdateLessonPlanTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/lesson-plan-templates")]
[ApiController]
[Authorize]
public class LessonPlanTemplateController : ControllerBase
{
    private readonly ISender _mediator;

    public LessonPlanTemplateController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tạo Lesson Plan Template mới
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateLessonPlanTemplate(
        [FromBody] CreateLessonPlanTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLessonPlanTemplateCommand
        {
            ProgramId = request.ProgramId,
            Level = request.Level,
            SessionIndex = request.SessionIndex,
            Attachment = request.Attachment
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/lesson-plan-templates/{r.Id}");
    }

    /// <summary>
    /// Lấy Lesson Plan Template theo ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetLessonPlanTemplateById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetLessonPlanTemplateByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Lấy danh sách Lesson Plan Templates với filter và pagination
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetLessonPlanTemplates(
        [FromQuery] Guid? programId,
        [FromQuery] string? level,
        [FromQuery] bool? isActive,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLessonPlanTemplatesQuery
        {
            ProgramId = programId,
            Level = level,
            IsActive = isActive,
            IncludeDeleted = includeDeleted,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Cập nhật Lesson Plan Template
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> UpdateLessonPlanTemplate(
        Guid id,
        [FromBody] UpdateLessonPlanTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLessonPlanTemplateCommand
        {
            Id = id,
            Level = request.Level,
            SessionIndex = request.SessionIndex,
            Attachment = request.Attachment,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Xóa Lesson Plan Template (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> DeleteLessonPlanTemplate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteLessonPlanTemplateCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

