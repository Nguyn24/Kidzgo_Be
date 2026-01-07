using Kidzgo.API.Extensions;
using Kidzgo.Application.Classes.GetStudentClasses;
using Kidzgo.Application.Sessions.GetStudentTimetable;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/students")]
[ApiController]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly ISender _mediator;

    public StudentController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách lớp của Student
    /// </summary>
    [HttpGet("{studentId:guid}/classes")]
    public async Task<IResult> GetClasses(
        Guid studentId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentClassesQuery
        {
            StudentId = studentId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Lấy thời khóa biểu của Student
    /// </summary>
    [HttpGet("{studentId:guid}/timetable")]
    public async Task<IResult> GetTimetable(
        Guid studentId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentTimetableQuery
        {
            StudentId = studentId,
            From = from,
            To = to
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

