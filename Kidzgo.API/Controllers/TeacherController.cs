using Kidzgo.API.Extensions;
using Kidzgo.Application.Classes.GetTeacherClasses;
using Kidzgo.Application.Classes.GetTeacherClassStudents;
using Kidzgo.Application.Sessions.GetTeacherTimetable;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/teacher")]
[ApiController]
[Authorize(Roles = "Teacher")]
public class TeacherController : ControllerBase
{
    private readonly ISender _mediator;

    public TeacherController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách lớp của Teacher
    /// </summary>
    [HttpGet("classes")]
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

    /// <summary>
    /// Lấy danh sách học sinh trong 1 lớp mà Teacher đang đảm nhận
    /// </summary>
    [HttpGet("classes/{classId:guid}/students")]
    public async Task<IResult> GetClassStudents(
        Guid classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherClassStudentsQuery
        {
            ClassId = classId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Lấy thời khóa biểu của Teacher
    /// </summary>
    [HttpGet("timetable")]
    public async Task<IResult> GetTimetable(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherTimetableQuery
        {
            From = from,
            To = to
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

