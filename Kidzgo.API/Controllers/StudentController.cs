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

    /// Lấy danh sách lớp của Student (studentId lấy từ token)
    [HttpGet("classes")]
    public async Task<IResult> GetClasses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentClassesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Lấy thời khóa biểu của Student (studentId lấy từ token)
    [HttpGet("timetable")]
    public async Task<IResult> GetTimetable(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentTimetableQuery
        {
            From = from,
            To = to
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

